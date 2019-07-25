using System.Net;
using System.Runtime.InteropServices;

namespace Dotnet.Container
{
    internal static class CredentialHelper
    {
        private static readonly string _defaultRegistry = "https://index.docker.io/v1/";

        public static bool TryGetCredentials(out NetworkCredential credential)
        {
            return TryGetCredentials(_defaultRegistry, out credential!);
        }

        public static bool TryGetCredentials(string registry, out NetworkCredential? credential)
        {
            credential = default;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (WindowsCredentials.CredRead(registry, WindowsCredentials.CRED_TYPE.GENERIC, 0, out var credPtr))
                {
                    var cred = (WindowsCredentials.CREDENTIAL)Marshal.PtrToStructure(credPtr, typeof(WindowsCredentials.CREDENTIAL))!;
                    credential = new NetworkCredential(userName: cred.userName,
                                                       password: Marshal.PtrToStringAnsi(cred.credentialBlob, cred.credentialBlobSize));
                    return true;
                }
            }

            // TODO: Retrieve credentials stored in OS X keychain

            return false;
        }
    }
}
