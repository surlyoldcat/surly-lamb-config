using System.Collections.Generic;

namespace Surly.LambConfig
{
    public class ServiceRegistryEntry 
    {
        public virtual string Service { get; set; }
        public virtual string Version { get; set; }
        public virtual string Status { get; set; }
        public virtual string Url { get; set; }
        public virtual string Port { get; set; }
        public virtual string Stage { get; set; }
        public virtual string Host { get; set; }
        
    }
}