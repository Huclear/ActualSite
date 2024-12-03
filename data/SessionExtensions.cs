using Newtonsoft.Json;

namespace ActualSite.data
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T? GetObject<T>(this ISession session, string key)
        {
            var str = session.GetString(key);
            return str == null ? default(T) : JsonConvert.DeserializeObject<T>(str);
        }
    }
}
