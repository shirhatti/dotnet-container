using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Dotnet.Container.Options;
using Dotnet.Container.RegistryClient;

namespace Dotnet.Container.CommandHandlers
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
                if (CredentialHelper.TryGetCredentials(registry, out var credential))
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

            var registryUri = new UriBuilder("https", registry).Uri;
            var registryInstance = new Registry(registryUri, username, password);

            var digest = await registryInstance.GetDigestFromReference("helloworld", "latest", ManifestType.OciV1);
            var appManifest = await registryInstance.GetManifestAsync("helloworld", digest, ManifestType.DockerV2);
            var appDiffId = appManifest.Layers[0].Annotations["io.deis.oras.content.digest"];
            var appSize = appManifest.Layers[0].Size;
            var appDigest = appManifest.Layers[0].Digest;

            var baseManifest = await registryInstance.GetManifestAsync("helloworld", "base", ManifestType.DockerV2);
            var baseConfigDigest = baseManifest.Config.Digest;


            var config = await registryInstance.GetConfigBlobAsync("helloworld", baseConfigDigest);

            // Modify config and POST it back
            config.rootfs.diff_ids.Add(appDiffId);
            (var newconfigSize, var newConfigSHA) = await registryInstance.PostConfigBlobAsync("helloworld", config);

            var layer = new Layer()
            {
                MediaType = "application/vnd.docker.image.rootfs.diff.tar.gzip",
                Size = appSize,
                Digest = appDigest
            };
            baseManifest.Layers.Add(layer);
            baseManifest.Config.Digest = newConfigSHA;
            baseManifest.Config.Size = newconfigSize;

            await registryInstance.PutManifestAsync("helloworld", "run3", baseManifest);
        }
    }
}
