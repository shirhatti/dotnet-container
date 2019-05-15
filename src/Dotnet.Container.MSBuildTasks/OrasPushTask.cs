using Dotnet.Container.Helpers;
using Microsoft.Build.Framework;
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

            Log.LogMessage(MessageImportance.High, ArgumentEscaper.EscapeAndConcatenate(args));

            using (var proc = Process.Start(psi))
            {
                proc.WaitForExit();
                Log.LogMessage(MessageImportance.High, proc.StandardOutput.ReadToEnd());
                Log.LogMessage(MessageImportance.High, proc.StandardError.ReadToEnd());
            }
            return true;
        }
    }
}
