using System.Collections.Generic;

namespace Surly.LambConfig
{
    
    public class LambdaMapping : ResourceMapping
    {
        public IEnumerable<LambdaEventMapping> Events { get; set; }
    }
}