using Newtonsoft.Json;
using System.Collections.Generic;

namespace Dotnet.Container.RegistryClient
{
    public class Layer
    {
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }
        [JsonProperty("size")]
        public long Size { get; set; }
        [JsonProperty("digest")]
        public string Digest { get; set; }
        [JsonProperty("annotations")]
        public Dictionary<string, string> Annotations { get; set; }
    }
}