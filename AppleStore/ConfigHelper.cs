using System.IO;
using Newtonsoft.Json.Linq;

namespace AppleStore
{
    public static class ConfigHelper
    {
        public static string GetConnectionString()
        {
            string json = File.ReadAllText("appsettings.json");
            JObject config = JObject.Parse(json);
            return (string)config["ConnectionStrings"]["DefaultConnection"];
        }
        public static string GetServerUrl()
        {
            string json = File.ReadAllText("appsettings.json");
            JObject config = JObject.Parse(json);
            return (string)config["ServerSettings"]["BaseUrl"];
        }
        public static string GetEmailSetting(string key)
        {
            string json = File.ReadAllText("appsettings.json");
            JObject config = JObject.Parse(json);
            return (string)config["EmailSettings"][key];
        }
    }
}