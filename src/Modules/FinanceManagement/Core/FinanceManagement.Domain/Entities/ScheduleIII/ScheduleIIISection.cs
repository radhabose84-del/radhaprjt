using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Global catalog of Schedule III sections (Current Assets, Income, ...). Not scoped to a master.
    public class ScheduleIIISection : BaseEntity, IActivityTracked
    {
        public string? SectionName { get; set; }
        public int StatementTypeId { get; set; }        // same-module FK -> Finance.MiscMaster (S3_STMT_TYPE)
        public int NatureId { get; set; }               // same-module FK -> Finance.MiscMaster (S3_NATURE)
        public int DisplayOrder { get; set; }           // statutory Schedule III sequence (drives section order in /structure)

        // Same-module FK navigations
        public MiscMaster? StatementType { get; set; }
        public MiscMaster? Nature { get; set; }

        // Inverse navigation
        public ICollection<ScheduleIIISectionItem>? LineItems { get; set; }
    }
}
