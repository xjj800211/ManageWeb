using System.Configuration;
using System.Xml;

namespace RM.Common.DotNetConfig
{
    public class ConfigHelper
    {
        public static string GetAppSettings(string key)
        {
            return ConfigurationManager.AppSettings[key].ToString().Trim();
        }

        public static void SetValue(XmlDocument xmlDocument, string selectPath, string key, string keyValue)
        {
            XmlNodeList xmlNodeList = xmlDocument.SelectNodes(selectPath);
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                if (xmlNode.Attributes["key"].Value.ToUpper().Equals(key.ToUpper()))
                {
                    xmlNode.Attributes["value"].Value = keyValue;
                    break;
                }
            }
        }
    }
}