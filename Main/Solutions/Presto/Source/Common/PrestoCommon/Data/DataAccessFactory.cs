using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace PrestoCommon.Data
{
    /// <summary>
    /// 
    /// </summary>
    public static class DataAccessFactory
    {
        private static string _namespace = GetDataNamespace();

        private static string GetDataNamespace()
        {
            // We want to use db4o or RavenDb, based on what's in the app.config.
            return ConfigurationManager.AppSettings["databaseNamespace"];
        }

        /// <summary>
        /// Gets the data interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDataInterface<T>() where T : class
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            
            T theObject = (from t in assembly.GetTypes()
                           where t.GetInterfaces().Contains(typeof(T))
                             && t.GetConstructor(Type.EmptyTypes) != null
                             && t.Namespace == _namespace
                           select Activator.CreateInstance(t) as T).FirstOrDefault() as T;

            return theObject as T;
        }
    }
}
