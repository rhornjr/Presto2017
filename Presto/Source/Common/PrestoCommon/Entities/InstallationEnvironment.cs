
namespace PrestoCommon.Entities
{
    public class InstallationEnvironment : EntityBase
    {
        public string Name { get; set; }
        public int LogicalOrder { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
