using Dotnet.Container.Helpers;
using dotnet_container.RegistryTypes;
using Microsoft.Build.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Dotnet.Container.MSBuildTasks
{
    public class OrasPush : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string OrasExe { get; set; }

        [Required]
        public string ImageName { get; set; }

        [Required]
        public string PublishDir { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        public override bool Execute()
        {
            var args = new[]
            {
                ""
            };
            var registryName = ImageName.Split('/').First();
            var repoName = ImageName.Substring(ImageName.IndexOf('/')+1).Split(':').First();
            var tagName = ImageName.Split(':').Last();
            var psi = new ProcessStartInfo
            {
                FileName = OrasExe,
                Arguments = ArgumentEscaper.EscapeAndConcatenate(args),
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            psi.EnvironmentVariables.Add("DOCKER_USERNAME", UserName);
            psi.EnvironmentVariables.Add("DOCKER_PASSWORD", Password);
            psi.EnvironmentVariables.Add("REGISTRY", registryName);
            psi.EnvironmentVariables.Add("REPO", repoName);
            psi.EnvironmentVariables.Add("TAG", tagName);
            psi.EnvironmentVariables.Add("PUBLISH_DIR", PublishDir);
            using (var proc = Process.Start(psi))
            {
                proc.WaitForExit();
                Log.LogMessage(MessageImportance.High, proc.StandardError.ReadToEnd());

                var bufferedOutput = proc.StandardOutput.ReadToEnd();
                Log.LogMessage(MessageImportance.High, bufferedOutput);

            }
            return true;
        }
    }
}
