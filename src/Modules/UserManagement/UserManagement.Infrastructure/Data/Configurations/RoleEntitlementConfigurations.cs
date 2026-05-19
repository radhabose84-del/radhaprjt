// This configuration is no longer registered in ApplicationDbContext.
// The AppSecurity.RoleEntitlements table is superseded by AppSecurity.RoleMenuPrivilege.
// Run migration: dotnet ef migrations add RemoveLegacyRoleEntitlements --startup-project ../../../BSOFT.Api
// Then: dotnet ef database update --startup-project ../../../BSOFT.Api
