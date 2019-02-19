#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;

#endregion

namespace Oblivion.Core
{
    public class ConfigurationData
    {
        public Dictionary<string,string> data;

        public ConfigurationData(string filePath, bool maynotexist = false)
        {
            data = new Dictionary<string, string>();

            if (!File.Exists(filePath))
                if (!maynotexist)
                    throw new ArgumentException("Unable to locate configuration file at '" + filePath + "'.");
                else
                    return;

            try
            {
                using (var stream = new StreamReader(filePath))
                {
                    string line;

                    while ((line = stream.ReadLine()) != null)
                    {
                        if (line.Length < 1 || line.StartsWith("#"))
                            continue;

                        var delimiterIndex = line.IndexOf('=');

                        if (delimiterIndex != -1)
                        {
                            var key = line.Substring(0, delimiterIndex);
                            var val = line.Substring(delimiterIndex + 1);

                            data.Add(key, val);
                        }
                    }
                }
            }

            catch (Exception e)
            {
                throw new ArgumentException("Could not process configuration file: " + e.Message);
            }
        }
    }

    public class ConfigData
    {
        public Dictionary<string, string> DBData;

        public ConfigData()
        {
            DBData = new Dictionary<string, string>();
            DBData.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `server_settings`");
                var ConfigData = dbClient.getTable();

                if (ConfigData == null) return;
                foreach (DataRow Data in ConfigData.Rows)
                    DBData.Add(Data[0].ToString(), Data[1].ToString());
            }
        }
    }
}