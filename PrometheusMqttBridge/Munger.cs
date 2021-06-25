namespace PrometheusMqttBridge
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class Munger
    {
        public static bool BoolToInt(ref string message)
        {
            if (message == null)
            {
                return false;
            }
            
            switch (message.Trim().ToLowerInvariant())
            {
                case "on":
                case "true":
                case "1":
                    message = "1";
                    return true;
                case "off":
                case "false":
                case "0":
                    message = "0";
                    return true;
                default:
                    return false;
            }
        }

        public static bool Div100(ref double value)
        {
            value = value / 100;
            return true;
        }
        
        public static bool JsonPath(ref string message, string path)
        {
            try
            {
                message = (string) JObject.Parse(message).SelectToken(path);
                return true;
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}; parsing {message} for {path}");
                return false;
            }
        }
    }
}