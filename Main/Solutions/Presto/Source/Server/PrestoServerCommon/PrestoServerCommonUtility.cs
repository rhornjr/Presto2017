using System;
using System.Diagnostics;
using System.Reflection;

namespace PrestoServerCommon
{
    public class PrestoServerCommonUtility
    {
        /// <summary>
        /// Gets the file version. This method will not throw an exception and it will not return null.
        /// </summary>
        /// <returns></returns>
        public static string GetFileVersion(Assembly assembly)
        {
            try
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                return fileVersionInfo.ProductVersion;
            }
            catch (Exception ex)
            {
                ServerCommonLogUtility.LogException(ex);
                return string.Empty;
            }
        }
    }
}
