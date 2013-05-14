using System;
using System.Globalization;
using PrestoCommon.Entities;
using PrestoServer.Data;
using PrestoServer.Data.Interfaces;
using Xanico.Core.Wcf;

namespace PrestoServer.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class LogicBase
    {
        /// <summary>
        /// Deletes the specified object to delete.
        /// </summary>
        /// <param name="objectToDelete">The object to delete.</param>
        public static void Delete(EntityBase objectToDelete)
        {
            DataAccessFactory.GetDataInterface<IGenericData>().Delete(objectToDelete);
        }

        internal static void SetConcurrencyUserSafeMessage(Exception ex, string itemName)
        {
            ex.Data[ExceptionDataKey.UserSafeMessage] =
                    string.Format(CultureInfo.CurrentCulture, PrestoServerResources.ItemCannotBeSavedConcurrency, itemName);
        }
    }
}
