using System;
using System.Dynamic;

namespace Surly.LambConfig
{
    /// <summary>
    /// This class is used by consumer code to access the loaded configuration.
    /// Config gets populated from ConfigBuilder.
    /// </summary>
    public static class LambConfiguration
    {
        private static object _lockObj = new object();
        private static LambConfigDocument _configDoc;
        
        public static LambConfigDocument Config
        {
            get
            {
                if (null == _configDoc)
                    throw new ApplicationException("Configuration has not been loaded");

                return _configDoc;
            }
        }

        internal static void InitConfig(LambConfigDocument doc)
        {
            if (null != _configDoc)
                throw new ApplicationException("Config is readonly, and has already been initialized");

            lock (_lockObj)
            {
                if (null == _configDoc)
                    _configDoc = doc;
            }
        }
    }
}