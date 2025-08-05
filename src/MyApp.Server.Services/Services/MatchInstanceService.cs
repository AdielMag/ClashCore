using System;
using System.Threading.Tasks;
using Google.Cloud.Run.V2;
using Google.Api.Gax.ResourceNames;
using Microsoft.Extensions.Logging;
using Server.Mongo.Collection;
using Server.Mongo.Entity;

namespace Server.Services
{
    public class MatchInstanceService
    {
        private readonly IMatchInstanceCollection _instances;
        private readonly ILogger<MatchInstanceService> _logger;
        private readonly int _capacityPerInstance;

        public MatchInstanceService(IMatchInstanceCollection instances,
                                     ILogger<MatchInstanceService> logger,
                                     int capacityPerInstance = 100)
        {
            _instances = instances;
            _logger = logger;
            _capacityPerInstance = capacityPerInstance;
        }

        public async Task<MatchInstance> AllocateInstanceAsync(int requiredSlots)
        {
            var instance = await _instances.TryAllocateInstanceAsync(_capacityPerInstance, requiredSlots);
            if (instance != null)
            {
                return instance;
            }

            instance = await ProvisionNewInstanceAsync();
            instance.PlayerCount = requiredSlots;
            return instance;
        }

        private async Task<MatchInstance> ProvisionNewInstanceAsync()
        {
            var projectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? "clashcore";
            var region = Environment.GetEnvironmentVariable("GCP_REGION") ?? "us-central1";
            var image = Environment.GetEnvironmentVariable("GAMEHUB_IMAGE") ??
                         $"{region}-docker.pkg.dev/{projectId}/clashcore/clashcore-gamehub:latest";
            var parent = LocationName.FromProjectLocation(projectId, region);
            var serviceId = $"gamehub-{Guid.NewGuid():N}";

            var serviceAccount = Environment.GetEnvironmentVariable("SERVICE_ACCOUNT") ?? 
                                 "cloudrun-runtime@clashcore.iam.gserviceaccount.com";

            var mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(mongoConnectionString))
            {
                throw new InvalidOperationException("MONGO_DB_CONNECTION_STRING environment variable is required for provisioning GameHub instances.");
            }
            
            var port = 8080; // Example port, can be configured as needed
            var client = await ServicesClient.CreateAsync();
            var request = new CreateServiceRequest
            {
                Parent = parent.ToString(),
                ServiceId = serviceId,
                Service = new Service
                {
                    Ingress = Google.Cloud.Run.V2.IngressTraffic.All,
                    Template = new RevisionTemplate
                    {
                        ServiceAccount = serviceAccount,
                        Containers =
                        {
                            new Google.Cloud.Run.V2.Container
                            {
                                Image = image,
                                Ports = { new ContainerPort { ContainerPort_ = port } },
                                Env =
                                {
                                    new EnvVar
                                    {
                                        Name = "MONGO_DB_CONNECTION_STRING",
                                        Value = mongoConnectionString
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var operation = await client.CreateServiceAsync(request);
            var completedOperation = await operation.PollUntilCompletedAsync();
            var service = completedOperation.Result;
            var url = service.Uri ?? string.Empty;

            // Set IAM policy to allow unauthenticated access using IAM API
            await SetIamPolicyForUnauthenticatedAccessAsync(service.Name);

            return await _instances.CreateInstanceAsync(url, port);
        }

        private async System.Threading.Tasks.Task SetIamPolicyForUnauthenticatedAccessAsync(string serviceName)
        {
            try
            {
                // Extract service ID and project/region from the full service name
                // serviceName format: projects/{project}/locations/{location}/services/{service}
                var parts = serviceName.Split('/');
                if (parts.Length >= 6)
                {
                    var projectId = parts[1];
                    var region = parts[3];
                    var serviceId = parts[5];

                    var processStartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "gcloud",
                        Arguments = $"run services add-iam-policy-binding {serviceId} " +
                                   $"--region={region} " +
                                   $"--member=\"allUsers\" " +
                                   $"--role=\"roles/run.invoker\" " +
                                   $"--project={projectId} " +
                                   $"--quiet",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using var process = System.Diagnostics.Process.Start(processStartInfo);
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        
                        if (process.ExitCode != 0)
                        {
                            var error = await process.StandardError.ReadToEndAsync();
                            _logger.LogWarning("Failed to set IAM policy for service {ServiceName}: {Error}", serviceName, error);
                        }
                        else
                        {
                            _logger.LogInformation("Successfully set IAM policy for unauthenticated access on service {ServiceName}", serviceName);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid service name format: {ServiceName}", serviceName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while setting IAM policy for service {ServiceName}", serviceName);
            }
        }
    }
}
