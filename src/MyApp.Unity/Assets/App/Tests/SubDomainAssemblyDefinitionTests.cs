using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace App.Tests
{
    public class SubDomainAssemblyDefinitionTests
    {
        [Test]
        public void EverySubDomainFolder_HasAssemblyDefinition()
        {
            // Find all SubDomains folders under Assets/App
            var appPath = Path.Combine(Application.dataPath, "App");
            var subDomainFolders = Directory.GetDirectories(appPath, "SubDomains", SearchOption.AllDirectories);
            Assert.IsNotEmpty(subDomainFolders, "No SubDomains folders found under Assets/App");

            var missing = new System.Collections.Generic.List<string>();
            foreach (var subDomains in subDomainFolders)
            {
                var subFolders = Directory.GetDirectories(subDomains);
                foreach (var folder in subFolders)
                {
                    var asmdefExists = Directory.GetFiles(folder, "*.asmdef", SearchOption.TopDirectoryOnly).Any();
                    if (!asmdefExists)
                        missing.Add(folder);
                }
            }

            if (missing.Count > 0)
            {
                Debug.LogError($"Missing assembly definitions in the following folders:\n{string.Join("\n", missing)}");
            }
            Assert.IsEmpty(missing, $"Missing assembly definition in: {string.Join(", ", missing)}");
        }
    }
}

