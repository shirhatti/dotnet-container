namespace Dotnet.Container.RegistryClient
{
    public class ManifestType
    {
        public string MediaType { get; }
        private ManifestType(string value)
        {
            MediaType = value;
        }
        public static ManifestType DockerV2 { get { return new ManifestType("application/vnd.docker.distribution.manifest.v2+json"); } }
        public static ManifestType OciV1 { get { return new ManifestType("application/vnd.oci.image.manifest.v1+json"); } }
    }
}