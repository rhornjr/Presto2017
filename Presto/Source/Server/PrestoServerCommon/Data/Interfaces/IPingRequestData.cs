using PrestoCommon.Entities;

namespace PrestoServer.Data.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPingRequestData
    {
        /// <summary>
        /// Saves the specified ping request.
        /// </summary>
        /// <param name="pingRequest">The ping request.</param>
        void Save(PingRequest pingRequest);

        /// <summary>
        /// Gets the most recent.
        /// </summary>
        /// <returns></returns>
        PingRequest GetMostRecent();
    }
}
