using Dotnet.Container.Options;
using Dotnet.Container.RegistryClient;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Dotnet.Container.CommandHandlers
{
    internal class ImportBaseCommandHandler
    {
        public static Command CreateCommand() => new Command(
            name: "import-base",
            description: "",
            symbols: new Option[]
            {
                RegistryOption.Create(),
                UsernameOption.Create(),
                PasswordOption.Create(),
                RepositoryOption.Create()
            },
            handler: CommandHandler.Create<IConsole, string?, string?, string?, string?>(ImportBaseAsync),
            isHidden: true
            );

        private static async Task ImportBaseAsync(IConsole console, string? registry, string? username, string? password, string? repository)
        {
            try
            {
                RegistryOption.EnsureNotNullorMalformed(registry);
                UsernameOption.EnsureNotNull(ref username);
                PasswordOption.EnsureNotNull(ref password);
                RepositoryOption.EnsureNotNullorMalformed(repository);
            }
            catch (ArgumentException e)
            {
                console.Error.WriteLine($"Push failed due to bad/missing argument:\t{e.ParamName}");
                return;
            }

            var mcrRegistry = new Registry(new Uri("https://mcr.microsoft.com"));
            var registryUri = new UriBuilder("https", registry).Uri;
            var registryInstance = new Registry(registryUri, username, password);

            // TODO: Accept as parameter
            var dotnetVersion = "2.2";

            Manifest manifest;
            switch(dotnetVersion)
            {
                case "2.1":
                    manifest = await mcrRegistry.GetManifestAsync("dotnet/core/runtime", "2.1", ManifestType.DockerV2);
                    break;
                case "2.2":
                    manifest = await mcrRegistry.GetManifestAsync("dotnet/core/runtime", "2.2", ManifestType.DockerV2);
                    break;
                case "3.0":
                    manifest = await mcrRegistry.GetManifestAsync("/dotnet/core-nightly/runtime", "3.0", ManifestType.DockerV2);
                    break;
                default:
                    manifest = await mcrRegistry.GetManifestAsync("dotnet/core-nightly/runtime-deps", "latest", ManifestType.DockerV2);
                    break;
            }

            foreach(var layer in manifest.Layers)
            {
                if(!await registryInstance.CheckIfLayerExistsAsync(repository, layer.Digest))
                {
                    // Layer doesn't exist. We'll need to upload it
                    await registryInstance.CopyBlobAsync(repository, layer.Digest, mcrRegistry, "dotnet/core/runtime");
                }
            }
            
        }
    }
}
