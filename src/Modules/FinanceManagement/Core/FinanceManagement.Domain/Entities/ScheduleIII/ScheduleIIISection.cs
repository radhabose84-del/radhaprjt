using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class ScheduleIIISection : BaseEntity
    {
        public int StructureId { get; set; }            // same-module FK -> ScheduleIIIStructure
        public string? SectionName { get; set; }
        public int StatementTypeId { get; set; }        // same-module FK -> Finance.MiscMaster (S3_STMT_TYPE)
        public int NatureId { get; set; }               // same-module FK -> Finance.MiscMaster (S3_NATURE)
        public int DisplayOrder { get; set; }

        // Same-module FK navigations
        public ScheduleIIIStructure? Structure { get; set; }
        public MiscMaster? StatementType { get; set; }
        public MiscMaster? Nature { get; set; }

        // Inverse navigation
        public ICollection<ScheduleIIILineItem>? LineItems { get; set; }
    }
}
