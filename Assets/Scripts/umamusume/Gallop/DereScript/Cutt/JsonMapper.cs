using LitJson;
using System;

namespace Cutt
{
    public static class JsonMapper
    {
        static JsonMapper()
        {
            LitJson.JsonMapper.RegisterExporter(delegate (float obj, LitJson.JsonWriter writer)
            {
                writer.Write(Convert.ToDouble(obj));
            });
            LitJson.JsonMapper.RegisterImporter((double input) => Convert.ToSingle(input));
            LitJson.JsonMapper.RegisterImporter((int input) => Convert.ToInt64(input));
        }

        public static JsonData ToObject(string json)
        {
            return LitJson.JsonMapper.ToObject(json);
        }

        public static T ToObject<T>(string json)
        {
            return LitJson.JsonMapper.ToObject<T>(json);
        }

        public static string ToJson(object obj)
        {
            return LitJson.JsonMapper.ToJson(obj);
        }
    }
}
