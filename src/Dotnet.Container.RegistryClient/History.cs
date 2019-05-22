using System;

namespace Dotnet.Container.RegistryClient
{
    public class History
    {
        public DateTime created { get; set; }
        public string created_by { get; set; }
        public bool empty_layer { get; set; }
    }

}
