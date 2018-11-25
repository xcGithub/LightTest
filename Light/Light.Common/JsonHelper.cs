using Newtonsoft.Json;
using Newtonsoft.Json.Converters; 

namespace xxoo.Common
{
    public class JsonHelper
    {
        public static string SerializeObject(object o)
        {
            return JsonConvert.SerializeObject(o);
        }
        public static string DateSerializeObject(object o, string timeFormatStr = "yyyy-MM-dd HH:mm:ss")
        {
            IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
            timeFormat.DateTimeFormat = timeFormatStr;
            return JsonConvert.SerializeObject(o, timeFormat);
        }
        public static T DeserializeObject<T>(string o)
        {
            return JsonConvert.DeserializeObject<T>(o);
        } 
         
    }
}