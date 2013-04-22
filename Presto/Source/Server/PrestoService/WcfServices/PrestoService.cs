using PrestoCommon.Interfaces;

namespace PrestoWcfService.WcfServices
{
    public class PrestoService : IPrestoService
    {
        public string Echo(string message)
        {
            return message;
        }
    }
}
