
namespace PrestoCommon.DataTransferObjects
{
    public class ApplicationServerDtoSlim
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string InstallationEnvironment { get; set; }
        public bool Archived { get; set; }
    }
}
