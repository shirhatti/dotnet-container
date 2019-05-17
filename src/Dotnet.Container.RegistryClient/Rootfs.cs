using System.Collections.Generic;

namespace Dotnet.Container.RegistryClient
{
    public class Rootfs
    {
        public string type { get; set; }
        public IList<string> diff_ids { get; set; }
    }

}
