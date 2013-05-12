using Microsoft.Practices.Unity;
using PrestoServer.Data.RavenDb;

namespace PrestoServer.Data
{
    public static class DataAccessFactory
    {
        public static T GetDataInterface<T>() where T : class
        {
            T theObject = PrestoServerUtility.Container.Resolve<T>();

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
