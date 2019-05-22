﻿using Dotnet.Container.Helpers;
using Dotnet.Container.Options;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Process = System.Diagnostics.Process;

namespace Dotnet.Container.CommandHandlers
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
            handler: CommandHandler.Create<IConsole, string?, string?, string?, string?>(Push),
            isHidden: false
            );

        private static void Push(IConsole console, string? registry, string? username, string? password, string? repository)
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
                   $"/p:RegistryPassword={password}"
            };
            var psi = new ProcessStartInfo
            {
                FileName = DotnetMuxer.MuxerPathOrDefault(),
                Arguments = ArgumentEscaper.EscapeAndConcatenate(args),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
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
                    throw new ApplicationException("Fatal error: could not find Oras.targets");
                }
                return targetPath;
            }
        }
    }
}