using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class YarnTwistMaster : BaseEntity
    {
        public string? TwistName { get; set; }
        public string? Description { get; set; }
    }
}
