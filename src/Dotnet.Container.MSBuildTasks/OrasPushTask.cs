using System.Diagnostics;
using System.Linq;
using Dotnet.Container.Helpers;
using Microsoft.Build.Framework;

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
                   "push",
                   "-u",
                   UserName,
                   "-p",
                   Password,
                   ImageName,
                   PublishDir
            };
            var psi = new ProcessStartInfo
            {
                FileName = OrasExe,
                Arguments = ArgumentEscaper.EscapeAndConcatenate(args),
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var proc = Process.Start(psi))
            {
                proc.WaitForExit();
                Log.LogMessage(MessageImportance.High, proc.StandardError.ReadToEnd());

                var bufferedOutput = proc.StandardOutput.ReadToEnd();
                Log.LogMessage(MessageImportance.High, bufferedOutput);
                var shaLine = bufferedOutput.Split('\n').Where(s => s.Contains("Digest: sha256:")).FirstOrDefault();
                var layerSha = shaLine.Substring(shaLine.IndexOf("sha256"));
                var registryName = ImageName.Split('/').First();
                var repoName = ImageName.Substring(ImageName.IndexOf('/') + 1).Split(':').First();
                var tagName = ImageName.Split(':').Last();

                Log.LogMessage(MessageImportance.High, registryName);
                Log.LogMessage(MessageImportance.High, repoName);
                Log.LogMessage(MessageImportance.High, tagName);

            }
            return true;
        }
    }
}
