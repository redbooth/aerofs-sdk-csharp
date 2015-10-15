using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Serialization;

namespace AeroFSSDK.Impl
{
    class DelegateContractResolver : IContractResolver
    {
        public IDictionary<Type, IContractResolver> Resolvers { get; set; }
        public IContractResolver Default { get; set; }
            = new DefaultContractResolver();

        public JsonContract ResolveContract(Type type)
        {
            IContractResolver resolver = Resolvers.TryGetValue(type, out resolver) ? resolver : Default;
            return resolver.ResolveContract(type);
        }
    }
}
