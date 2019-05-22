using System;

namespace Dotnet.Container.RegistryClient
{

    public class ConfigurationManifest
    {
        public string architecture { get; set; }
        public Config config { get; set; }
        public string container { get; set; }
        public Container_Config container_config { get; set; }
        public DateTime created { get; set; }
        public string docker_version { get; set; }
        public History[] history { get; set; }
        public string os { get; set; }
        public Rootfs rootfs { get; set; }
    }

}
