
namespace SalesManagement.Application.SalesSegment.Dto
{
    public class SalesSegmentDto
    {
        // Primary Key
        public int Id { get; set; }

        // Composite Key Fields
        public int SalesOrganisationId { get; set; }
        public string? SalesOrganisationName { get; set; }

        public int SalesChannelId { get; set; }
        public string? SalesChannelName { get; set; }

        public int BusinessUnitId { get; set; }
        public string? BusinessUnitName { get; set; }

        // Optional Fields
        public int? CurrencyId { get; set; }
        public string? CurrencyName { get; set; }

        public DateTime? ValidFrom { get; set; }
        public string? SegmentName { get; set; }

        // Status
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Audit Fields
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
