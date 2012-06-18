using System;
using System.Collections.Generic;

namespace PrestoCommon.Factories
{
    public static class TypeContainer
    {
        private static Dictionary<Type, Type> _interfaceToConcreteMapping = new Dictionary<Type, Type>();

        public static void RegisterType(Type interfaceType, Type concreteType)
        {
            _interfaceToConcreteMapping.Add(interfaceType, concreteType);
        }

        public static T RetrieveType<T>() where T : class
        {
            return Activator.CreateInstance(_interfaceToConcreteMapping[typeof(T)]) as T;
        }
    }
}
