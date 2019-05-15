using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dotnet_container.RegistryTypes
{
    public class Manifest
    {
        [JsonProperty("schemaVersion")]
        public long SchemaVersion { get; set; }
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }
        [JsonProperty("config")]
        public Config Config { get; set; }
        [JsonProperty("layers")]
        public IList<Config> Layers { get; set; }
    }
}
