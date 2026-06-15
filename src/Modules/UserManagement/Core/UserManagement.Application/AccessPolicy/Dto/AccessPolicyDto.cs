namespace UserManagement.Application.AccessPolicy.Dto
{
    public class AccessPolicyDto
    {
        public int     Id         { get; set; }
        public string  PolicyCode { get; set; } = string.Empty;
        public string  PolicyName { get; set; } = string.Empty;
        public string  EntityName { get; set; } = string.Empty;
        public string  FieldName  { get; set; } = string.Empty;
        public bool    IsActive   { get; set; }
        public bool    IsDeleted  { get; set; }
        public int     CreatedBy  { get; set; }
        public DateTime? CreatedAt  { get; set; }
        public string? CreatedByName { get; set; }
        public int?    ModifiedBy { get; set; }
        public DateTime? ModifiedAt  { get; set; }
    }
}
