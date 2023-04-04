using Domain.Enums;

namespace Domain.Entities
{
    public class Permissions
    {
        public List<PermissionType> Spot { get; set; }
        public List<PermissionType> Margin { get; set; }
        public List<PermissionType> Futures { get; set; }
        public List<PermissionType> Lending { get; set; }
        public List<PermissionType> LeveragedToken { get; set; }
    }
}
