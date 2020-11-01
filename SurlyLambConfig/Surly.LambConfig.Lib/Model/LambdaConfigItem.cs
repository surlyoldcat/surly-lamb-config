using System.Collections.Generic;

namespace Surly.LambConfig
{
    
    public class LambdaConfigItem : NetworkConfigItem
    {
        public List<LambdaEventConfigItem> Events { get; set; }
    }
}