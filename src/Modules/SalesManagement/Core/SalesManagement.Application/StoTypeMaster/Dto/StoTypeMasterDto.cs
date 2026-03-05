namespace SalesManagement.Application.StoTypeMaster.Dto
{
    public class StoTypeMasterDto
    {
        public int Id { get; set; }
        public string? StoTypeCode { get; set; }
        public string? StoTypeName { get; set; }
        public string? Description { get; set; }

        // PGI Movement Type (from JOIN)
        public int PgiMovementTypeId { get; set; }
        public string? PgiMovementCode { get; set; }
        public string? PgiMovementDescription { get; set; }

        // GR Movement Type (from JOIN)
        public int GrMovementTypeId { get; set; }
        public string? GrMovementCode { get; set; }
        public string? GrMovementDescription { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Audit fields
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
