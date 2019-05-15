using dotnet_container.Options;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dotnet.Container.Helpers;
using Process = System.Diagnostics.Process;

namespace dotnet_container
{
    internal class PushCommandHandler
    {
        public static Command CreateCommand() => new Command(
            name: "push",
            description: "Invoke dotnet publish and push resulting artifact to registry",
            symbols: new Option[]
            {
                RegistryOption.Create(),
                UsernameOption.Create(),
                PasswordOption.Create(),
                RepositoryOption.Create()
            },
            handler: CommandHandler.Create<IConsole, string?, string?, string?, string?>(PushAsync),
            isHidden: true
            );

        private static void PushAsync(IConsole console, string? registry, string? username, string? password, string? repository)
        {
            RegistryOption.EnsureNotNullorMalformed(registry);
            UsernameOption.EnsureNotNull(ref username);
            PasswordOption.EnsureNotNull(ref password);
            RepositoryOption.EnsureNotNullorMalformed(repository);

            var finder = new MsBuildProjectFinder(Environment.CurrentDirectory);
            var projectFile = finder.FindMsBuildProject();
            var targetsFile = FindTargetsFile();

            var args = new[]
            {
                   "msbuild",
                   projectFile,
                   "/nologo",
                   "/restore",
                   "/t:Publish",
                   $"/p:CustomAfterMicrosoftCommonTargets={targetsFile}",
                   $"/p:CustomAfterMicrosoftCommonCrossTargetingTargets={targetsFile}",
                   $"/p:ImageName={registry}/{repository}",
                   $"/p:RegistryUsername={username}",
                   $"/p:RegistryPassword={password}",
                   "/bl"
            };
            var psi = new ProcessStartInfo
            {
                FileName = DotNetMuxer.MuxerPathOrDefault(),
                Arguments = ArgumentEscaper.EscapeAndConcatenate(args),
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start(psi);
            process.WaitForExit();
            console.Out.WriteLine(process.StandardOutput.ReadToEnd());

            string FindTargetsFile()
            {
                var assemblyDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                var searchPaths = new[]
                {
                   Path.Combine(AppContext.BaseDirectory, "assets"),
                   Path.Combine(assemblyDir, "assets"),
                   AppContext.BaseDirectory,
                   assemblyDir,
               };

                var targetPath = searchPaths.Select(p => Path.Combine(p, "Oras.targets")).FirstOrDefault(File.Exists);
                if (targetPath == null)
                {
                    Console.WriteLine("Fatal error: could not find Oras.targets");
                }
                return targetPath;
            }
        }
    }
}