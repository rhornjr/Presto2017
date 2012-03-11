﻿using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using PrestoCommon.Data.RavenDb;

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

            DataAccessLayerBase theObjectAsRavenDalBase = theObject as DataAccessLayerBase;

            // HACK: We want one session per logic call, and this is how we're doing it. Only the logic classes call this
            //       method. The data classes call each other directly (if that's even necessary, and I don't think it is).
            if (theObjectAsRavenDalBase != null)
            {
                theObjectAsRavenDalBase.SetAsInitialDalInstanceAndCreateSession();
            }

            return theObject as T;
        }
    }
}
