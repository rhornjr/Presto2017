using System;
using System.Web.Optimization;

[assembly: WebActivator.PostApplicationStartMethod(
    typeof(PrestoWeb.App_Start.HotTowelConfig), "PreStart")]

namespace PrestoWeb.App_Start
{
    public static class HotTowelConfig
    {
        public static void PreStart()
        {
            // Add your start logic here
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}