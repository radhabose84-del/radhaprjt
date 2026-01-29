// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Contracts.Events.Users;
// using Core.Domain.Entities;
// using MassTransit;
// using Microsoft.EntityFrameworkCore;

// // ---- Disambiguate entity types to avoid namespace conflicts ----
// using UserEntity = Core.Domain.Entities.User;
// using UserRoleAllocationEntity = Core.Domain.Entities.UserRoleAllocation;
// using UserGroupEntity = Core.Domain.Entities.UserGroup;
// using UserCompanyEntity = Core.Domain.Entities.UserCompany;
// using UserUnitEntity = Core.Domain.Entities.UserUnit;
// using CompanyEntity = Core.Domain.Entities.Company;   // maps AppData.Company
// using UnitEntity = Core.Domain.Entities.Unit;
// using Core.Application.Common.Interfaces;
// using Microsoft.Extensions.Logging;         // maps AppData.Unit



// namespace Core.Application.Consumers
// {
//     public class PartyApprovedConsumer : IConsumer<PartyApprovedIntegrationEvent>
//     {
//         private readonly DbContext _db;
//         private readonly IIPAddressService _iPAddressService;
//         private readonly IEmailService _emailService;
//         private readonly ILogger<PartyApprovedConsumer> _logger;


//         public PartyApprovedConsumer(DbContext db, IIPAddressService iPAddressService, IEmailService emailService, ILogger<PartyApprovedConsumer> logger)
//         {
//             _db = db;
//             _iPAddressService = iPAddressService;
//             _emailService = emailService;
//             _logger = logger;
//         }

//         public async Task Consume(ConsumeContext<PartyApprovedIntegrationEvent> context)
//         {
//             var ct = context.CancellationToken;
//             var m = context.Message;

//             var users = _db.Set<User>();

//             // Idempotency: if a user already exists for this party, stop.
//             if (await _db.Set<User>().AsNoTracking().AnyAsync(u => u.PartyId == m.PartyId, ct))
//                 return;

//             // Username
//             var baseUserName = !string.IsNullOrWhiteSpace(m.Email)
//                 ? m.Email!.Trim().ToLowerInvariant()
//                 : !string.IsNullOrWhiteSpace(m.PartyCode) ? m.PartyCode!.Trim()
//                 : $"party{m.PartyId}";
//             var userName = await EnsureUniqueUserNameAsync(baseUserName);

//             // Audit headers




//             // int createdBy = 0;
//             // if (context.Headers.TryGetHeader("user-id", out var uidObj) &&
//             //     int.TryParse(uidObj?.ToString(), out var uidParsed))
//             //     createdBy = uidParsed;

//             // var createdByName = context.Headers.TryGetHeader("user-name", out var unameObj)
//             //     ? unameObj?.ToString() ?? "System"
//             //     : "System";

//             // var createdIp = context.Headers.TryGetHeader("ip", out var ipObj)
//             //     ? ipObj?.ToString() ?? "0.0.0.0"
//             //     : "0.0.0.0";

//             // resolve audit values (payload -> headers -> defaults)
//             var (createdBy, createdByName, createdIp, createdAt) = ResolveAudit(context, m, _iPAddressService);

//             // Build user
//             var user = new User
//             {
//                 Id = Guid.NewGuid(),
//                 FirstName = m.PartyName,
//                 LastName = m.PartyLastName,
//                 UserName = userName,
//                 IsFirstTimeUser = Core.Domain.Enums.Common.Enums.FirstTimeUserStatus.Yes,
//                 Mobile = m.Mobile,
//                 EmailId = m.Email,
//                 IsLocked = 0,
//                 PartyId = m.PartyId,
//                 // UserType = Core.Domain.Enums.Common.MiscEnumEntity.UserType.External,
//                 // Base entity audit (if your BaseEntity fields are not auto-populated by interceptors)
//                 CreatedBy = createdBy,
//                 CreatedByName = createdByName,
//                 CreatedIP = createdIp,
//                 CreatedAt = createdAt,
//                 IsActive = Core.Domain.Enums.Common.Enums.Status.Active,
//                 IsDeleted = Core.Domain.Enums.Common.Enums.IsDelete.NotDeleted
//             };

//             // === MULTI COMPANIES & UNITS (FK-safe) ===

//             // Normalize incoming
//             var requestedCompanyIds = (m.CompanyIds ?? Array.Empty<int>()).Where(i => i > 0).Distinct().ToList();
//             var requestedUnitIds = (m.UnitIds ?? Array.Empty<int>()).Where(i => i > 0).Distinct().ToList();

//             // Validate companies
//             var validCompanyIds = requestedCompanyIds.Count == 0
//                 ? new List<int>()
//                 : await _db.Set<CompanyEntity>()
//                     .AsNoTracking()
//                     .Where(c => requestedCompanyIds.Contains(c.Id))
//                     .Select(c => c.Id)
//                     .ToListAsync();
//             // include default company if provided and valid
//             if (m.DefaultCompanyId.HasValue &&
//                 !validCompanyIds.Contains(m.DefaultCompanyId.Value) &&
//                 await _db.Set<CompanyEntity>().AsNoTracking().AnyAsync(c => c.Id == m.DefaultCompanyId.Value))
//             {
//                 validCompanyIds.Add(m.DefaultCompanyId.Value);
//             }

//             user.UserCompanies = validCompanyIds
//                 .Distinct()
//                 .Select(id => new UserCompanyEntity { CompanyId = id, IsActive = 1 })
//                 .ToList();


//             // Validate units
//             var validUnitIds = requestedUnitIds.Count == 0
//                 ? new List<int>()
//                 : await _db.Set<UnitEntity>()
//                     .AsNoTracking()
//                     .Where(u => requestedUnitIds.Contains(u.Id))
//                     .Select(u => u.Id)
//                     .ToListAsync();

//             // include default unit if provided and valid
//             if (m.DefaultUnitId.HasValue &&
//                 !validUnitIds.Contains(m.DefaultUnitId.Value) &&
//                 await _db.Set<UnitEntity>().AsNoTracking().AnyAsync(u => u.Id == m.DefaultUnitId.Value))
//             {
//                 validUnitIds.Add(m.DefaultUnitId.Value);
//             }

//             user.UserUnits = validUnitIds
//                 .Distinct()
//                 .Select(id => new UserUnitEntity { UnitId = id, IsActive = 1 })
//                 .ToList();

//             // ===== Roles (MANDATORY, never create) =====
//             // Map party type codes coming from the event to role names
//             var desiredRoleNames = MapPartyTypesToRoleNames(m.PartyTypeCodes ?? Array.Empty<string>());

//             // Collect role IDs that MUST exist in AppSecurity.UserRole for each company
//             var ensuredRoleIds = new List<int>();
//             if (desiredRoleNames.Count > 0 && user.UserCompanies.Any())
//             {
//                 var companyIds = user.UserCompanies.Select(c => c.CompanyId).Distinct().ToList();

//                 // Throw if any (CompanyId, RoleName) is missing
//                 ensuredRoleIds = await GetExistingRolesForCompaniesOrThrowAsync(companyIds, desiredRoleNames);

//                 user.UserRoleAllocations ??= new List<UserRoleAllocationEntity>();
//                 foreach (var roleId in ensuredRoleIds)
//                 {
//                     if (!user.UserRoleAllocations.Any(a => a.UserRoleId == roleId))
//                         user.UserRoleAllocations.Add(new UserRoleAllocationEntity { UserRoleId = roleId, IsActive = 1 });
//                 }
//             }

//             // Optional: also honor DefaultRoleId if provided
//             if (m.DefaultRoleId.HasValue)
//             {
//                 user.UserRoleAllocations ??= new List<UserRoleAllocationEntity>();
//                 if (!user.UserRoleAllocations.Any(a => a.UserRoleId == m.DefaultRoleId.Value))
//                     user.UserRoleAllocations.Add(new UserRoleAllocationEntity { UserRoleId = m.DefaultRoleId.Value, IsActive = 1 });
//             }


//             // Password (BCrypt via domain method)
//             var tempPassword = $"P@rty{Guid.NewGuid():N}".Substring(0, 12);
//             user.SetPassword(tempPassword);


//             user.UserType = await ResolveUserTypeInternalAsync(context.CancellationToken);                // e.g., "Internal"
//             user.EntityId = await ResolveEntityIdFromCompaniesAsync(user.UserCompanies, context.CancellationToken);
//             user.UserGroupId = await ResolveDefaultUserGroupIdAsync(context.CancellationToken);


//             // Save: let EF execution strategy handle retries (no manual BeginTransaction)
//             users.Add(user);
//             await _db.SaveChangesAsync();

//             // -------- FIRST-TIME EMAIL (using your IEmailService) --------
//             if (!string.IsNullOrWhiteSpace(user.EmailId))
//             {
//                 try
//                 {
//                     // choose provider the same way you do in ForgotUserPassword
//                     var provider = user.EmailId.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
//                         ? "Gmail"
//                         : "Zimbra";

//                     // compose email
//                     var subject = "Your portal account is ready";
//                     var html = $@"
//                         <p>Dear {(string.IsNullOrWhiteSpace(user.FirstName) ? "User" : user.FirstName)},</p>
//                         <p>Your portal access has been created.</p>
//                         <p><b>Username:</b> {user.UserName}<br/>
//                            <b>Temporary password:</b> {tempPassword}</p>
//                         <p>Please sign in and change your password immediately.</p>
//                         <p>Thanks,<br/>Support Team</p>";

//                     var emailCmd = new Contracts.Events.Notifications.SendEmailCommand
//                     {
//                         ToEmail = user.EmailId,
//                         Subject = subject,
//                         HtmlContent = html,
//                         Provider = provider
//                     };

//                     var sent = await _emailService.SendEmailAsync(emailCmd);
//                     if (sent)
//                         _logger.LogInformation("Welcome email sent to {Email} for PartyId {PartyId}", user.EmailId, m.PartyId);
//                     else
//                         _logger.LogWarning("Failed to send welcome email to {Email} for PartyId {PartyId}", user.EmailId, m.PartyId);
//                 }
//                 catch (Exception ex)
//                 {
//                     // log only – don’t throw, to avoid MassTransit re-executing the consumer
//                     _logger.LogError(ex, "Error sending welcome email to {Email} for PartyId {PartyId}", user.EmailId, m.PartyId);
//                 }
//             }
//         }

//         private async Task<string> EnsureUniqueUserNameAsync(string baseName)
//         {
//             var users = _db.Set<User>();
//             var candidate = baseName; var i = 0;

//             while (await users.AsNoTracking().AnyAsync(u => u.UserName == candidate))
//                 candidate = $"{baseName}{++i}";

//             return candidate;
//         }
//         private async Task<int?> ResolveUserGroupIdAsync(string desiredGroupName)
//         {
//             // exact, case-insensitive
//             var idByName = await _db.Set<UserGroupEntity>()
//                 .AsNoTracking()
//                 .Where(g => g.IsActive == Core.Domain.Enums.Common.Enums.Status.Active && g.GroupName != null)
//                 .Where(g => g.GroupName!.Trim().ToLower() == desiredGroupName.Trim().ToLower())
//                 .Select(g => (int?)g.Id)
//                 .FirstOrDefaultAsync();

//             if (idByName.HasValue)
//                 return idByName;

//             // fallback heuristic
//             var nameLower = desiredGroupName.Trim().ToLower();
//             var idByHeuristic = await _db.Set<UserGroupEntity>()
//                 .AsNoTracking()
//                 .Where(g => g.IsActive == Core.Domain.Enums.Common.Enums.Status.Active && g.GroupCode == "USER" && g.GroupName != null)
//                 .Where(g =>
//                     g.GroupName!.ToLower().Contains("super") &&
//                     g.GroupName!.ToLower().Contains("admin") &&
//                     g.GroupName!.ToLower().Contains("user"))
//                 .OrderBy(g => g.Id)
//                 .Select(g => (int?)g.Id)
//                 .FirstOrDefaultAsync();

//             return idByHeuristic;
//         }

//         // Map SUPPLIER/CUSTOMER/AGENT -> role names (you can adjust naming)
//         private static List<string> MapPartyTypesToRoleNames(IReadOnlyList<string> partyTypeCodes)
//         {
//             var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
//             foreach (var code in partyTypeCodes ?? Array.Empty<string>())
//             {
//                 var c = (code ?? "").Trim().ToUpperInvariant();
//                 if (c is "SUPPLIER" or "CUSTOMER" or "AGENT") set.Add(c);
//             }
//             return set.ToList();
//         }

//         private async Task<List<int>> GetExistingRolesForCompaniesOrThrowAsync(
//             IReadOnlyList<int> companyIds,
//             IReadOnlyList<string> roleNames)
//         {
//             var roleIds = new List<int>();

//             if (companyIds == null || companyIds.Count == 0 || roleNames == null || roleNames.Count == 0)
//                 return roleIds;

//             var existing = await _db.Set<Core.Domain.Entities.UserRole>()
//                 .AsNoTracking()
//                 .Where(r => companyIds.Contains(r.CompanyId) && roleNames.Contains(r.RoleName))
//                 .Select(r => new { r.Id, r.RoleName, r.CompanyId })
//                 .ToListAsync();

//             var existingLookup = existing
//                 .GroupBy(e => (e.CompanyId, e.RoleName))
//                 .ToDictionary(g => g.Key, g => g.Select(x => x.Id).First());

//             var missing = new List<(int CompanyId, string RoleName)>();

//             foreach (var companyId in companyIds)
//             {
//                 foreach (var roleName in roleNames)
//                 {
//                     if (existingLookup.TryGetValue((companyId, roleName), out var id))
//                         roleIds.Add(id);
//                     else
//                         missing.Add((companyId, roleName));
//                 }
//             }

//             if (missing.Count > 0)
//             {
//                 var msg = "Required roles are missing in AppSecurity.UserRole: " +
//                           string.Join(", ", missing.Select(m => $"(CompanyId={m.CompanyId}, RoleName='{m.RoleName}')"));
//                 throw new InvalidOperationException(msg);
//             }

//             return roleIds.Distinct().ToList();
//         }


//         private async Task<int> ResolveUserTypeInternalAsync(CancellationToken ct)
//         {
//             const string miscTypeCode = Domain.Enums.Common.MiscEnumEntity.UserType.MiscTypeCode;
//             const string code = Domain.Enums.Common.MiscEnumEntity.UserType.External;

//             // 1) Find MiscTypeId for "UserType"
//             var miscTypeId = await _db.Set<Core.Domain.Entities.MiscTypeMaster>()
//                 .AsNoTracking()
//                 .Where(t => t.MiscTypeCode == miscTypeCode)
//                 .Select(t => (int?)t.Id)
//                 .FirstOrDefaultAsync(ct);

//             if (miscTypeId is null)
//                 return 1; // fallback

//             // 2) Find the MiscMaster row for "Internal" within that type
//             var id = await _db.Set<Core.Domain.Entities.MiscMaster>()
//                 .AsNoTracking()
//                 .Where(x => x.MiscTypeId == miscTypeId.Value
//                         && x.Code == code
//                         && x.IsActive == Domain.Enums.Common.Enums.Status.Active
//                         && x.IsDeleted == 0)
//                 .OrderBy(x => x.Id)
//                 .Select(x => (int?)x.Id)
//                 .FirstOrDefaultAsync(ct);

//             return id ?? 1;
//         }

//         private async Task<int?> ResolveEntityIdFromCompaniesAsync(IList<UserCompanyEntity>? userCompanies, CancellationToken ct)
//         {
//             if (userCompanies == null || userCompanies.Count == 0)
//                 return null;

//             var firstCompanyId = userCompanies.Select(c => c.CompanyId).FirstOrDefault();
//             if (firstCompanyId <= 0)
//                 return null;

//             // CompanyEntity should expose EntityId (as in your AppData.Company table)
//             var entityId = await _db.Set<CompanyEntity>()
//                 .AsNoTracking()
//                 .Where(c => c.Id == firstCompanyId)
//                 .Select(c => (int?)c.EntityId)
//                 .FirstOrDefaultAsync(ct);

//             return entityId; // can be null if company row doesn't have it
//         }

//         private async Task<int?> ResolveDefaultUserGroupIdAsync(CancellationToken ct)
//         {
//             // Try exact by GroupCode
//             var idByCode = await _db.Set<UserGroupEntity>()
//                 .AsNoTracking()
//                 .Where(g => g.IsActive == Domain.Enums.Common.Enums.Status.Active)
//                 .Where(g => g.GroupCode == "USER")
//                 .OrderBy(g => g.Id)
//                 .Select(g => (int?)g.Id)
//                 .FirstOrDefaultAsync(ct);

//             if (idByCode.HasValue)
//                 return idByCode;

//             // Fallback: any active group whose name contains "User"
//             var idByName = await _db.Set<UserGroupEntity>()
//                 .AsNoTracking()
//                 .Where(g => g.IsActive == Domain.Enums.Common.Enums.Status.Active)
//                 .Where(g => g.GroupName != null && g.GroupName.Contains("User"))
//                 .OrderBy(g => g.Id)
//                 .Select(g => (int?)g.Id)
//                 .FirstOrDefaultAsync(ct);

//             return idByName; // can be null; DB column allows null per your dump
//         }
            
//             // 1) put this helper in your consumer file
//         private static (int createdBy, string createdByName, string createdIp, DateTime createdAt)
//             ResolveAudit(ConsumeContext<PartyApprovedIntegrationEvent> ctx,
//                         PartyApprovedIntegrationEvent m,
//                         IIPAddressService ipSvc)
//         {
//             // prefer PAYLOAD (publisher filled these)
//             var by   = m.CreatedBy;
//             var byNm = m.CreatedByName;
//             var ip   = m.CreatedIp;
//             var at   = m.CreatedAt == default ? DateTime.UtcNow : m.CreatedAt;

//             // then HEADERS (if payload not present)
//             if (by == 0 && ctx.Headers.TryGetHeader("user-id", out var uidObj)
//                 && int.TryParse(uidObj?.ToString(), out var uidParsed))
//                 by = uidParsed;

//             if (string.IsNullOrWhiteSpace(byNm) &&
//                 ctx.Headers.TryGetHeader("user-name", out var unameObj))
//                 byNm = unameObj?.ToString();

//             if (string.IsNullOrWhiteSpace(ip) &&
//                 ctx.Headers.TryGetHeader("ip", out var ipObj))
//                 ip = ipObj?.ToString();

//             // finally LOCAL defaults
//             if (by == 0) by = ipSvc.GetUserId();
//             if (string.IsNullOrWhiteSpace(byNm)) byNm = ipSvc.GetUserName() ?? "System";
//             if (string.IsNullOrWhiteSpace(ip)) ip = ipSvc.GetSystemIPAddress() ?? "0.0.0.0";

//             return (by, byNm, ip, at);
//         }

                    
       
//     }
// }
