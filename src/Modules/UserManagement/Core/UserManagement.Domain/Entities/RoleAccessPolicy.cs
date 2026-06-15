namespace UserManagement.Domain.Entities
{
    /// <summary>
    /// Maps a Role to an allowed value ID for a given AccessPolicy.
    /// No audit fields — this is a pure join/config table managed by admins.
    /// </summary>
    public class RoleAccessPolicy
    {
        public int Id             { get; set; }
        public int AccessPolicyId { get; set; }
        public int RoleId         { get; set; }
        public int ValueId        { get; set; }

        public AccessPolicy? AccessPolicy { get; set; }
        public UserRole?     UserRole     { get; set; }
    }
}
