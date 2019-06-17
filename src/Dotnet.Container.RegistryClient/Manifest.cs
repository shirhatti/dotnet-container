﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dotnet.Container.RegistryClient
{
    public class Manifest
    {
        [JsonProperty("schemaVersion")]
        public long SchemaVersion { get; set; }
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }
        [JsonProperty("config")]
        public Layer Config { get; set; }
        [JsonProperty("layers")]
        public IList<Layer> Layers { get; set; }
    }
}
