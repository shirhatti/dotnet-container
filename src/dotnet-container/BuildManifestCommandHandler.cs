using dotnet_container.Options;
using dotnet_container.RegistryTypes;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace dotnet_container
{
    internal class BuildManifestCommandHandler
    {
        public static Command CreateCommand() => new Command(
            name: "build-manifest",
            description: "",
            symbols: new Option[]
            {
                RegistryOption.Create(),
                UsernameOption.Create(),
                PasswordOption.Create(),
            },
            handler: CommandHandler.Create<IConsole, string?, string?, string?>(TestCredentialsAsync),
            isHidden: true
            );
        private static async Task TestCredentialsAsync(IConsole console, string? registry, string? username, string? password)
        {
            try
            {
                RegistryOption.EnsureNotNullorMalformed(registry);
                UsernameOption.EnsureNotNull(ref username);
                PasswordOption.EnsureNotNull(ref password);
            }
            catch (ArgumentException e)
            {
                console.Error.WriteLine($"Push failed due to bad/missing argument:\t{e.ParamName}");
                return;
            }

            var registryUri = new UriBuilder("https", registry).Uri;
            var registryInstance = new Registry(registryUri, username, password);
            var layerSHA = "sha256:e5796678b5e6d2d3b0a29a223504c13d6b1c8332405ce4b73aef8c90bd1f13dc";

            try
            {
                var appManifest = await registryInstance.GetManifestAsync("helloworld", layerSHA, ManifestType.DockerV2);
                var appDiffId = appManifest.Layers[0].Annotations["io.deis.oras.content.digest"];
                var appSize = appManifest.Layers[0].Size;
                var appDigest = appManifest.Layers[0].Digest;

                var baseManifest = await registryInstance.GetManifestAsync("aspnet", "3.0.0-preview5", ManifestType.DockerV2);
                var baseDigest = baseManifest.Config.Digest;


                var appLayer = new Config()
                {
                    
                };

            }
            catch (RegistryException)
            {

            }
        }
    }
}