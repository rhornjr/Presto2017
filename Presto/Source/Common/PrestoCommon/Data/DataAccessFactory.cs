using System;
using System.Linq;
using System.Reflection;

namespace PrestoCommon.Data
{
    /// <summary>
    /// 
    /// </summary>
    public static class DataAccessFactory
    {
        /// <summary>
        /// Gets the data interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDataInterface<T>() where T : class
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            // Note: objects will not be null if no types are found. An empty list will be returned.
            T theObject = (from t in assembly.GetTypes()
                           where t.GetInterfaces().Contains(typeof(T))
                             && t.GetConstructor(Type.EmptyTypes) != null
                           select Activator.CreateInstance(t) as T).FirstOrDefault() as T;

            return theObject as T;
        }
    }
}
