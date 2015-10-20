using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Serialization;

namespace AeroFSSDK.Impl
{
    class RemapPropertyNamesContractResolver : DefaultContractResolver
    {
        public IDictionary<string, string> PropertyMapping { get; set; }

        protected override string ResolvePropertyName(string propertyName)
        {
            string value;
            return PropertyMapping.TryGetValue(propertyName, out value)
                ? value
                : base.ResolvePropertyName(propertyName);
        }
    }
}
