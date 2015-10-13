using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace AeroFSSDK.Impl
{
    class StringIDReader<T> : JsonConverter where T : StringID, new()
    {
        public override bool CanRead { get; } = true;
        public override bool CanWrite { get; } = false;

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return new T { Base = reader.Value as string };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Writing StringID is not supported at the moment.");
        }
    }
}
