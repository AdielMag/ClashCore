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
            await _instances.CreateInstanceAsync(instance);
            return instance;
        }

        private async Task<MatchInstance> ProvisionNewInstanceAsync()
        {
            var projectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? "clashcore";
            var region = Environment.GetEnvironmentVariable("GCP_REGION") ?? "us-central1";
            var image = Environment.GetEnvironmentVariable("GAMEHUB_IMAGE") ??
                         $"{region}-docker.pkg.dev/{projectId}/clashcore-gamehub/clashcore-gamehub:latest";

            var parent = LocationName.FromProjectLocation(projectId, region);
            var serviceId = $"gamehub-{Guid.NewGuid():N}";

            var client = await ServicesClient.CreateAsync();
            var request = new CreateServiceRequest
            {
                Parent = parent.ToString(),
                ServiceId = serviceId,
                Service = new Service
                {
                    Template = new RevisionTemplate
                    {
                        Containers =
                        {
                            new Google.Cloud.Run.V2.Container
                            {
                                Image = image,
                                Ports = { new ContainerPort { ContainerPort_ = 12346 } }
                            }
                        }
                    }
                }
            };

            var op = await client.CreateServiceAsync(request);
            var result = await op.PollUntilCompletedAsync();
            var url = result.Result.Uri ?? string.Empty;

            return new MatchInstance
            {
                Url = url,
                Port = 12346,
                PlayerCount = 0
            };
        }
    }
}
