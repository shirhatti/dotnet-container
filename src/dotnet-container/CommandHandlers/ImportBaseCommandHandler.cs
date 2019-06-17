using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using Dotnet.Container.Options;
using Dotnet.Container.RegistryClient;

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
                RepositoryOption.EnsureNotNullorMalformed(repository);
            }
            catch (ArgumentException e)
            {
                console.Error.WriteLine($"Push failed due to bad/missing argument:\t{e.ParamName}");
                return;
            }

            try
            {
                UsernameOption.EnsureNotNull(ref username);
                PasswordOption.EnsureNotNull(ref password);
            }
            catch (ArgumentException e)
            {
                if (CredentialHelper.TryGetCredentials(registry!, out var credential))
                {
                    username = credential!.UserName;
                    password = credential!.Password;
                }
                else
                {
                    console.Error.WriteLine($"Push failed due to bad/missing argument:\t{e.ParamName}");
                    return;
                }
            }

            var mcrRegistry = new Registry(new Uri("https://mcr.microsoft.com"));
            var registryUri = new UriBuilder("https", registry).Uri;
            var registryInstance = new Registry(registryUri, username, password);

            // TODO: Accept as parameter
            var dotnetVersion = "2.2";

            Manifest manifest;
            switch (dotnetVersion)
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

            await Task.WhenAll(manifest.Layers.Select(layer => registryInstance.CopyBlobAsync(repository, layer.Digest, mcrRegistry, "dotnet/core/runtime")));
            await registryInstance.CopyBlobAsync(repository, manifest.Config.Digest, mcrRegistry, "dotnet/core/runtime");
            await registryInstance.PutManifestAsync(repository, "base", manifest);
        }
    }
}
