using Microsoft.Build.Framework;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MSBuildTasks
{
    public class OrasPush : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string OrasExe { get; set; }

        [Required]
        public string ImageName { get; set; }

        [Required]
        public string PublishDir { get; set; }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, ImageName);
            Log.LogMessage(MessageImportance.High, PublishDir);

            var psi = new ProcessStartInfo(fileName: OrasExe,
                                           arguments: "--help");
            psi.RedirectStandardOutput = true;
            using (var proc = Process.Start(psi))
            {
                proc.WaitForExit();
                Log.LogMessage(MessageImportance.High, proc.StandardOutput.ReadToEnd());
            }
            return true;
        }
    }
}
