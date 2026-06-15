namespace UserManagement.Application.AccessPolicy.Dto
{
    public class RoleAccessPolicyDto
    {
        public int     Id             { get; set; }
        public int     AccessPolicyId { get; set; }
        public string  PolicyCode     { get; set; } = string.Empty;
        public string  PolicyName     { get; set; } = string.Empty;
        public int     RoleId         { get; set; }
        public string? RoleName       { get; set; }
        public int     ValueId        { get; set; }
    }
}
