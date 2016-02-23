using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.EMR
{
    public static class EmrProtocol
    {
        public const string Odbc = "ODBC";
        public const string Jdbc = "JDBC";
    }

    public static class EmrInterfaceType
    {
        public const string Hive = "Hive";
        public const string HBase = "HBase";
        public const string Impala = "Impala";
    }

    public class EmrDeveloperOptions
    {
        public string Protocol { get; private set; }
        public string InterfaceType { get; private set; }
        public int Port { get; private set; }
        
        public static EmrDeveloperOptions Parse(IEnumerable<AddonParameter> devParameters)
        {
            var options = new EmrDeveloperOptions();

            foreach (var parameter in devParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(EmrDeveloperOptions existingOptions, string key, string value)
        {
            if (key.Equals("protocol"))
            {
                if (value.Equals(EmrProtocol.Odbc))
                {
                    existingOptions.Protocol = EmrProtocol.Odbc;
                    return;
                }
                if (value.Equals(EmrProtocol.Jdbc))
                {
                    existingOptions.Protocol = EmrProtocol.Jdbc;
                    return;
                }
                throw new ArgumentException("EMR Access Protocol either not supported or not understood. Check your syntax or contact your platform operator.");
            }
            if (key.Equals("interfacetype"))
            {
                if (value.Equals(EmrInterfaceType.HBase))
                {
                    existingOptions.InterfaceType = EmrInterfaceType.HBase;
                    // if 0 return the hbase default (60000)
                    if (existingOptions.Port.Equals(0)) existingOptions.Port = 60000;
                    return;
                }
                if (value.Equals(EmrInterfaceType.Hive))
                {
                    existingOptions.InterfaceType = EmrInterfaceType.Hive;
                    // if 0 return the hive default (10000)
                    if (existingOptions.Port.Equals(0)) existingOptions.Port = 10000;
                    return;
                }
                if (value.Equals(EmrInterfaceType.Impala))
                {
                    existingOptions.InterfaceType = EmrInterfaceType.Impala;
                    // if 0 return the impala default (21050)
                    if (existingOptions.Port.Equals(0)) existingOptions.Port = 21050;
                    return;
                }
                throw new ArgumentException("EMR Interface Type either not supported or not understood. Supported types: Hive, HBase, Impala");
            }
            if (key.Equals("port"))
            {
                int temp;
                if(int.TryParse(value, out temp)) throw new ArgumentException("Port must be a number.");
                existingOptions.Port = temp;
                return;
            }
            throw new ArgumentException("Parameter not supported. Check your syntax.");
        }
    }
}
