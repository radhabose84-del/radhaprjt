// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Contracts.Events.Users;
// using UserManagement.Application.Common.Interfaces;
// using MassTransit;
// using Microsoft.EntityFrameworkCore;


// // Short aliases so we don't collide with namespaces
// using UserEntity               = Core.Domain.Entities.User;
// using UserCompanyEntity        = Core.Domain.Entities.UserCompany;
// using UserUnitEntity           = Core.Domain.Entities.UserUnit;
// using UserRoleEntity           = Core.Domain.Entities.UserRole;
// using UserRoleAllocationEntity = Core.Domain.Entities.UserRoleAllocation;
// using CompanyEntity            = Core.Domain.Entities.Company;
// using UnitEntity               = Core.Domain.Entities.Unit;

// namespace Core.Application.Consumers
// {
//     /// <summary>
//     /// Keeps AppSecurity.Users in sync with Party changes.
//     /// If a user doesn't exist and IsPortalAccessEnabled == true -> creates the user.
//     /// Otherwise, syncs IsActive, Company/Unit memberships, and party-type roles.
//     /// </summary>
//     public class PartySyncConsumer : IConsumer<PartySyncIntegrationEvent>
//     {
//         private readonly DbContext _db;
//         private readonly IIPAddressService _ip;

//         public PartySyncConsumer(DbContext db, IIPAddressService ipAddressService)
//         {
//             _db = db;
//             _ip = ipAddressService;
//         }

//         public async Task Consume(ConsumeContext<PartySyncIntegrationEvent> context)
//         {
//             var ct = context.CancellationToken;
//             var m = context.Message;

//             // Try load existing user by PartyId
//             var user = await _db.Set<UserEntity>()
//                 .Include(u => u.UserCompanies)
//                 .Include(u => u.UserUnits)
//                 .Include(u => u.UserRoleAllocations)
//                 .FirstOrDefaultAsync(u => u.PartyId == m.PartyId, ct);

//             if (user is null)
//             {
//                 // No user yet -> create only if portal access is enabled
//                 if (!m.IsPortalAccessEnabled)
//                     return;

//                 var created = await TryCreateUserAsync(context, m, ct);
//                 if (!created)
//                     return; // someone else created it, or portal disabled

//                 // reload so the rest of the sync can run safely
//                 user = await _db.Set<UserEntity>()
//                     .Include(u => u.UserCompanies)
//                     .Include(u => u.UserUnits)
//                     .Include(u => u.UserRoleAllocations)
//                     .FirstOrDefaultAsync(u => u.PartyId == m.PartyId, ct);

//                 if (user is null) return;

//                 // await CreateUserFromSyncAsync(m, ct);
//                 // return;
//             }

//             // User exists -> sync fields
//             var (updatedBy, updatedByName, updatedIp) = GetAudit(context, m);

//             // Basic identity (optional)
//             if (!string.IsNullOrWhiteSpace(m.PartyName)) user.FirstName = m.PartyName;
//             if (!string.IsNullOrWhiteSpace(m.PartyLastName)) user.LastName = m.PartyLastName;
//             if (!string.IsNullOrWhiteSpace(m.Email)) user.EmailId = m.Email!.Trim();
//             if (!string.IsNullOrWhiteSpace(m.Mobile)) user.Mobile = m.Mobile!.Trim();

//             // IsActive based on IsPortalAccessEnabled
//             user.IsActive = m.IsPortalAccessEnabled
//                 ? Core.Domain.Enums.Common.Enums.Status.Active
//                 : Core.Domain.Enums.Common.Enums.Status.Inactive;
                
//             user.IsLocked = m.IsPortalAccessEnabled ? (byte)0 : (byte)1;    

//             // Audit
//             user.ModifiedBy = updatedBy;
//             user.ModifiedByName = updatedByName;
//             user.ModifiedIP = updatedIp;
//             user.ModifiedAt = DateTime.UtcNow;

//             // Companies & Units
//             var desiredCompanyIds = (m.CompanyIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
//             if (desiredCompanyIds.Count == 0 && m.DefaultCompanyId.GetValueOrDefault() > 0)
//                 desiredCompanyIds.Add(m.DefaultCompanyId.Value);

//             var desiredUnitIds = (m.UnitIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
//             if (desiredUnitIds.Count == 0 && m.DefaultUnitId.GetValueOrDefault() > 0)
//                 desiredUnitIds.Add(m.DefaultUnitId.Value);

//             await SyncUserCompaniesAsync(user, desiredCompanyIds, ct);
//             await SyncUserUnitsAsync(user, desiredUnitIds, ct);

//             // Party-type roles (e.g., SUPPLIER/CUSTOMER/AGENT)
//             var desiredRoleNames = (m.PartyTypeCodes ?? Array.Empty<string>())
//                 .Where(s => !string.IsNullOrWhiteSpace(s))
//                 .Select(s => s.Trim().ToUpperInvariant())
//                 .Distinct()
//                 .ToList();

//             await SyncUserRolesAsync(user, desiredRoleNames, ct);

//             await _db.SaveChangesAsync(ct);
//         }
//         private async Task<bool> TryCreateUserAsync(ConsumeContext<PartySyncIntegrationEvent> ctx, PartySyncIntegrationEvent m, CancellationToken ct)
//         {
//             // reduce race window: re-check right before insert
//             if (await _db.Set<UserEntity>().AsNoTracking().AnyAsync(u => u.PartyId == m.PartyId, ct))
//                 return false;

//             // Username from email or "party{id}"
//             var baseUserName = !string.IsNullOrWhiteSpace(m.Email)
//                 ? m.Email!.Trim().ToLowerInvariant()
//                 : $"party{m.PartyId}";
//             var userName = await EnsureUniqueUserNameAsync(baseUserName, ct);

//             // Audit from headers > payload > defaults
//             var createdBy = TryGetIntHeader(ctx, "user-id") ?? m.ModifiedBy ?? 0;
//             var createdByName = TryGetStrHeader(ctx, "user-name") ?? m.ModifiedByName ?? "System";
//             var createdIp = TryGetStrHeader(ctx, "ip") ?? m.ModifiedIp ?? "0.0.0.0";

//             var user = new UserEntity
//             {
//                 Id = Guid.NewGuid(),
//                 FirstName = string.IsNullOrWhiteSpace(m.PartyName) ? $"Party {m.PartyId}" : m.PartyName!,
//                 LastName = null,                                  // no LastName in the event; keep null
//                 UserName = userName,
//                 EmailId = m.Email,
//                 Mobile = m.Mobile,
//                 IsLocked =  m.IsPortalAccessEnabled ? (byte)0 : (byte)1,
//                 PartyId = m.PartyId,
//                 IsFirstTimeUser = Core.Domain.Enums.Common.Enums.FirstTimeUserStatus.Yes,
//                 CreatedBy = createdBy,
//                 CreatedByName = createdByName,
//                 CreatedIP = createdIp,
//                 CreatedAt = DateTime.UtcNow,
//                 IsActive = Core.Domain.Enums.Common.Enums.Status.Active,
//                 IsDeleted = Core.Domain.Enums.Common.Enums.IsDelete.NotDeleted
//             };

//             // Companies
//             var desiredCompanyIds = (m.CompanyIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
//             if (desiredCompanyIds.Count == 0 && m.DefaultCompanyId.GetValueOrDefault() > 0)
//                 desiredCompanyIds.Add(m.DefaultCompanyId.Value);

//             var validCompanyIds = desiredCompanyIds.Count == 0
//                 ? new List<int>()
//                 : await _db.Set<CompanyEntity>().AsNoTracking()
//                     .Where(c => desiredCompanyIds.Contains(c.Id))
//                     .Select(c => c.Id)
//                     .ToListAsync(ct);

//             user.UserCompanies = validCompanyIds.Select(id => new UserCompanyEntity { CompanyId = id, IsActive = 1 }).ToList();

//             // Units
//             var desiredUnitIds = (m.UnitIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
//             if (desiredUnitIds.Count == 0 && m.DefaultUnitId.GetValueOrDefault() > 0)
//                 desiredUnitIds.Add(m.DefaultUnitId.Value);

//             var validUnitIds = desiredUnitIds.Count == 0
//                 ? new List<int>()
//                 : await _db.Set<UnitEntity>().AsNoTracking()
//                     .Where(u => desiredUnitIds.Contains(u.Id))
//                     .Select(u => u.Id)
//                     .ToListAsync(ct);

//             user.UserUnits = validUnitIds.Select(id => new UserUnitEntity { UnitId = id, IsActive = 1 }).ToList();

//             // Roles (must already exist; we do not create roles here)
//             var desiredRoleNames = (m.PartyTypeCodes ?? Array.Empty<string>())
//                 .Where(s => !string.IsNullOrWhiteSpace(s))
//                 .Select(s => s.Trim().ToUpperInvariant())
//                 .Distinct()
//                 .ToList();

//             if (desiredRoleNames.Count > 0 && user.UserCompanies.Any())
//             {
//                 var companyIds = user.UserCompanies.Select(c => c.CompanyId).Distinct().ToList();

//                 var roleIds = await _db.Set<UserRoleEntity>().AsNoTracking()
//                     .Where(r => companyIds.Contains(r.CompanyId) &&
//                                 desiredRoleNames.Contains(r.RoleName.ToUpper()))
//                     .Select(r => r.Id)
//                     .ToListAsync(ct);

//                 user.UserRoleAllocations = roleIds
//                     .Select(rid => new UserRoleAllocationEntity { UserRoleId = rid, IsActive = 1 })
//                     .ToList();
//             }

//             // Password
//             var tempPassword = $"P@rty{Guid.NewGuid():N}".Substring(0, 12);
//             user.SetPassword(tempPassword);

//             _db.Add(user);
//             try
//             {
//                 await _db.SaveChangesAsync(ct);
//                 return true;
//             }
//             catch (DbUpdateException ex) when (IsUniqueViolation(ex))
//             {
//                 // Another concurrent consumer created the user first -> treat as idempotent
//                 return false;
//             }
//         }

//         // ------------------ sync helpers ------------------

//         private static (int updatedBy, string updatedByName, string updatedIp)
//             GetAudit(ConsumeContext<PartySyncIntegrationEvent> ctx, PartySyncIntegrationEvent m)
//         {
//             var userId = 0;
//             var uname = "System";
//             var ip = "0.0.0.0";

//             if (ctx.Headers.TryGetHeader("user-id", out var uidObj) &&
//                 int.TryParse(uidObj?.ToString(), out var parsedId))
//                 userId = parsedId;
//             else if (m.ModifiedBy.HasValue)
//                 userId = m.ModifiedBy.Value;

//             if (ctx.Headers.TryGetHeader("user-name", out var unameObj))
//                 uname = unameObj?.ToString() ?? uname;
//             else if (!string.IsNullOrWhiteSpace(m.ModifiedByName))
//                 uname = m.ModifiedByName!;

//             if (ctx.Headers.TryGetHeader("ip", out var ipObj))
//                 ip = ipObj?.ToString() ?? ip;
//             else if (!string.IsNullOrWhiteSpace(m.ModifiedIp))
//                 ip = m.ModifiedIp!;

//             return (userId, uname, ip);
//         }

//         private async Task SyncUserCompaniesAsync(UserEntity user, IReadOnlyList<int> desiredCompanyIds, CancellationToken ct)
//         {
//             var validIds = desiredCompanyIds.Count == 0
//                 ? new List<int>()
//                 : await _db.Set<CompanyEntity>().AsNoTracking()
//                     .Where(c => desiredCompanyIds.Contains(c.Id))
//                     .Select(c => c.Id)
//                     .ToListAsync(ct);

//             var desired = new HashSet<int>(validIds);
//             var current = new HashSet<int>(user.UserCompanies?.Select(x => x.CompanyId) ?? Enumerable.Empty<int>());

//             var toAdd = desired.Except(current).ToList();
//             var toRemove = current.Except(desired).ToList();

//             foreach (var cid in toAdd)
//                 user.UserCompanies!.Add(new UserCompanyEntity { CompanyId = cid, IsActive = 1 });

//             user.UserCompanies!.RemoveWhere(uc => toRemove.Contains(uc.CompanyId));
//         }

//         private async Task SyncUserUnitsAsync(UserEntity user, IReadOnlyList<int> desiredUnitIds, CancellationToken ct)
//         {
//             var validIds = desiredUnitIds.Count == 0
//                 ? new List<int>()
//                 : await _db.Set<UnitEntity>().AsNoTracking()
//                     .Where(u => desiredUnitIds.Contains(u.Id))
//                     .Select(u => u.Id)
//                     .ToListAsync(ct);

//             var desired = new HashSet<int>(validIds);
//             var current = new HashSet<int>(user.UserUnits?.Select(x => x.UnitId) ?? Enumerable.Empty<int>());

//             var toAdd = desired.Except(current).ToList();
//             var toRemove = current.Except(desired).ToList();

//             foreach (var uid in toAdd)
//                 user.UserUnits!.Add(new UserUnitEntity { UnitId = uid, IsActive = 1 });

//             user.UserUnits!.RemoveWhere(uu => toRemove.Contains(uu.UnitId));
//         }

//         private async Task SyncUserRolesAsync(UserEntity user, IReadOnlyList<string> desiredRoleNames, CancellationToken ct)
//         {
//             if (desiredRoleNames.Count == 0)
//                 return;

//             var companyIds = user.UserCompanies?.Select(c => c.CompanyId).Distinct().ToList() ?? new List<int>();
//             if (companyIds.Count == 0) return;

//             // Desired roles that exist in DB
//             var desiredRoles = await _db.Set<UserRoleEntity>().AsNoTracking()
//                 .Where(r => companyIds.Contains(r.CompanyId) && desiredRoleNames.Contains(r.RoleName.ToUpper()))
//                 .Select(r => new { r.Id, NameUpper = r.RoleName.ToUpper() })
//                 .ToListAsync(ct);

//             var desiredRoleIds = new HashSet<int>(desiredRoles.Select(r => r.Id));
//             var currentRoleIds = new HashSet<int>(user.UserRoleAllocations?.Select(a => a.UserRoleId) ?? Enumerable.Empty<int>());

//             // Add missing
//             foreach (var rid in desiredRoleIds.Except(currentRoleIds))
//                 user.UserRoleAllocations!.Add(new UserRoleAllocationEntity { UserRoleId = rid, IsActive = 1 });

//             // Deactivate party-type roles that are not desired now
//             var partyRoleIdsAll = await _db.Set<UserRoleEntity>().AsNoTracking()
//                 .Where(r => companyIds.Contains(r.CompanyId))
//                 .Where(r => r.RoleName != null && (
//                       r.RoleName.ToUpper() == "SUPPLIER"
//                    || r.RoleName.ToUpper() == "CUSTOMER"
//                    || r.RoleName.ToUpper() == "AGENT"))
//                 .Select(r => r.Id)
//                 .ToListAsync(ct);

//             foreach (var alloc in user.UserRoleAllocations!.Where(a => partyRoleIdsAll.Contains(a.UserRoleId)))
//                 alloc.IsActive = desiredRoleIds.Contains(alloc.UserRoleId) ? (byte)1 : (byte)0;
//         }

//         private async Task<string> EnsureUniqueUserNameAsync(string baseName, CancellationToken ct)
//         {
//             var users = _db.Set<UserEntity>();
//             var candidate = baseName;
//             var i = 0;
//             while (await users.AsNoTracking().AnyAsync(u => u.UserName == candidate, ct))
//                 candidate = $"{baseName}{++i}";
//             return candidate;
//         }

//         // ------------------ small helpers ------------------

//         private static int? TryGetIntHeader(ConsumeContext ctx, string key) =>
//             ctx.Headers.TryGetHeader(key, out var v) && int.TryParse(v?.ToString(), out var p) ? p : (int?)null;

//         private static string? TryGetStrHeader(ConsumeContext ctx, string key) =>
//             ctx.Headers.TryGetHeader(key, out var v) ? v?.ToString() : null;

//         private static bool IsUniqueViolation(DbUpdateException ex)
//         {
//             var msg = ex.InnerException?.Message ?? ex.Message;
//             return msg.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
//                 || msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
//                 || msg.Contains("UX_Users_PartyId", StringComparison.OrdinalIgnoreCase)
//                 || msg.Contains("IX_Users_PartyId", StringComparison.OrdinalIgnoreCase);
//         }
//     }

//     internal static class CollectionExtensions
//     {
//         public static void RemoveWhere<T>(this ICollection<T> source, Func<T, bool> predicate)
//         {
//             if (source == null || source.Count == 0) return;
//             var remove = source.Where(predicate).ToList();
//             foreach (var item in remove) source.Remove(item);
//         }
//     }

// }

//         // ------------------ create path ------------------

//     //     private async Task CreateUserFromSyncAsync(PartySyncIntegrationEvent m, CancellationToken ct)
//     //     {
//     //         // Username
//     //         var baseUserName = !string.IsNullOrWhiteSpace(m.Email)
//     //             ? m.Email!.Trim().ToLowerInvariant()
//     //             : $"party{m.PartyId}";
//     //         var userName = await EnsureUniqueUserNameAsync(baseUserName, ct);

//     //         // Build
//     //         var createdBy     = m.UpdatedBy      ?? 0;
//     //         var createdByName = m.UpdatedByName  ?? "System";
//     //         var createdIp     = string.IsNullOrWhiteSpace(m.UpdatedIp) ? "0.0.0.0" : m.UpdatedIp!;

//     //         var user = new UserEntity
//     //         {
//     //             Id               = Guid.NewGuid(),
//     //             FirstName        = string.IsNullOrWhiteSpace(m.PartyName) ? $"Party {m.PartyId}" : m.PartyName!,
//     //             LastName =       string.IsNullOrWhiteSpace(m.PartyLastName) ? "" : m.PartyName!,
//     //             UserName         = userName,
//     //             EmailId          = m.Email,
//     //             Mobile           = m.Mobile,
//     //             IsLocked         = 0,
//     //             PartyId          = m.PartyId,
//     //             IsFirstTimeUser  = Core.Domain.Enums.Common.Enums.FirstTimeUserStatus.Yes,
//     //             CreatedBy        = createdBy,
//     //             CreatedByName    = createdByName,
//     //             CreatedIP        = createdIp,
//     //             CreatedAt        = DateTime.UtcNow,
//     //             IsActive         = Core.Domain.Enums.Common.Enums.Status.Active,
//     //             IsDeleted        = Core.Domain.Enums.Common.Enums.IsDelete.NotDeleted
//     //         };

//     //         // Companies
//     //         var desiredCompanyIds = (m.CompanyIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
//     //         if (desiredCompanyIds.Count == 0 && m.DefaultCompanyId.GetValueOrDefault() > 0)
//     //             desiredCompanyIds.Add(m.DefaultCompanyId.Value);

//     //         var validCompanyIds = desiredCompanyIds.Count == 0
//     //             ? new List<int>()
//     //             : await _db.Set<CompanyEntity>().AsNoTracking()
//     //                 .Where(c => desiredCompanyIds.Contains(c.Id))
//     //                 .Select(c => c.Id)
//     //                 .ToListAsync(ct);

//     //         user.UserCompanies = validCompanyIds.Select(id => new UserCompanyEntity { CompanyId = id, IsActive = 1 }).ToList();

//     //         // Units
//     //         var desiredUnitIds = (m.UnitIds ?? Array.Empty<int>()).Where(x => x > 0).Distinct().ToList();
//     //         if (desiredUnitIds.Count == 0 && m.DefaultUnitId.GetValueOrDefault() > 0)
//     //             desiredUnitIds.Add(m.DefaultUnitId.Value);

//     //         var validUnitIds = desiredUnitIds.Count == 0
//     //             ? new List<int>()
//     //             : await _db.Set<UnitEntity>().AsNoTracking()
//     //                 .Where(u => desiredUnitIds.Contains(u.Id))
//     //                 .Select(u => u.Id)
//     //                 .ToListAsync(ct);

//     //         user.UserUnits = validUnitIds.Select(id => new UserUnitEntity { UnitId = id, IsActive = 1 }).ToList();

//     //         // Roles (from PartyTypeCodes). We **do not** create roles; they must exist.
//     //         var desiredRoleNames = (m.PartyTypeCodes ?? Array.Empty<string>())
//     //             .Where(s => !string.IsNullOrWhiteSpace(s))
//     //             .Select(s => s.Trim().ToUpperInvariant())
//     //             .Distinct()
//     //             .ToList();

//     //         if (desiredRoleNames.Count > 0 && user.UserCompanies.Any())
//     //         {
//     //             var companyIds = user.UserCompanies.Select(c => c.CompanyId).Distinct().ToList();

//     //             var roleIds = await _db.Set<UserRoleEntity>().AsNoTracking()
//     //                 .Where(r => companyIds.Contains(r.CompanyId) &&
//     //                             desiredRoleNames.Contains(r.RoleName.ToUpper()))
//     //                 .Select(r => r.Id)
//     //                 .ToListAsync(ct);

//     //             user.UserRoleAllocations = roleIds
//     //                 .Select(rid => new UserRoleAllocationEntity { UserRoleId = rid, IsActive = 1 })
//     //                 .ToList();
//     //         }

//     //         // Optional enrich (if you’ve implemented these resolvers already):
//     //         // user.UserType    = await ResolveUserTypeInternalAsync(ct);
//     //         // user.EntityId    = await ResolveEntityIdFromCompaniesAsync(user.UserCompanies, ct);
//     //         // user.UserGroupId = await ResolveDefaultUserGroupIdAsync(ct);

//     //         // Password
//     //         var tempPassword = $"P@rty{Guid.NewGuid():N}".Substring(0, 12);
//     //         user.SetPassword(tempPassword);

//     //         _db.Add(user);
//     //         await _db.SaveChangesAsync(ct);
//     //     }

//     //     // ------------------ sync helpers ------------------

//     //     private static (int updatedBy, string updatedByName, string updatedIp)
//     //         GetAudit(ConsumeContext<PartySyncIntegrationEvent> ctx, PartySyncIntegrationEvent m)
//     //     {
//     //         // prefer headers, then payload, then defaults
//     //         var userId = 0;
//     //         var uname  = "System";
//     //         var ip     = "0.0.0.0";

//     //         if (ctx.Headers.TryGetHeader("user-id", out var uidObj) &&
//     //             int.TryParse(uidObj?.ToString(), out var parsedId))
//     //             userId = parsedId;
//     //         else if (m.UpdatedBy.HasValue)
//     //             userId = m.UpdatedBy.Value;

//     //         if (ctx.Headers.TryGetHeader("user-name", out var unameObj))
//     //             uname = unameObj?.ToString() ?? uname;
//     //         else if (!string.IsNullOrWhiteSpace(m.UpdatedByName))
//     //             uname = m.UpdatedByName!;

//     //         if (ctx.Headers.TryGetHeader("ip", out var ipObj))
//     //             ip = ipObj?.ToString() ?? ip;
//     //         else if (!string.IsNullOrWhiteSpace(m.UpdatedIp))
//     //             ip = m.UpdatedIp!;

//     //         return (userId, uname, ip);
//     //     }

//     //     private async Task SyncUserCompaniesAsync(UserEntity user, IReadOnlyList<int> desiredCompanyIds, CancellationToken ct)
//     //     {
//     //         var validIds = desiredCompanyIds.Count == 0
//     //             ? new List<int>()
//     //             : await _db.Set<CompanyEntity>().AsNoTracking()
//     //                 .Where(c => desiredCompanyIds.Contains(c.Id))
//     //                 .Select(c => c.Id)
//     //                 .ToListAsync(ct);

//     //         var desired = new HashSet<int>(validIds);
//     //         var current = new HashSet<int>(user.UserCompanies?.Select(x => x.CompanyId) ?? Enumerable.Empty<int>());

//     //         var toAdd    = desired.Except(current).ToList();
//     //         var toRemove = current.Except(desired).ToList();

//     //         foreach (var cid in toAdd)
//     //             user.UserCompanies!.Add(new UserCompanyEntity { CompanyId = cid, IsActive = 1 });

//     //         user.UserCompanies!.RemoveWhere(uc => toRemove.Contains(uc.CompanyId));
//     //     }

//     //     private async Task SyncUserUnitsAsync(UserEntity user, IReadOnlyList<int> desiredUnitIds, CancellationToken ct)
//     //     {
//     //         var validIds = desiredUnitIds.Count == 0
//     //             ? new List<int>()
//     //             : await _db.Set<UnitEntity>().AsNoTracking()
//     //                 .Where(u => desiredUnitIds.Contains(u.Id))
//     //                 .Select(u => u.Id)
//     //                 .ToListAsync(ct);

//     //         var desired = new HashSet<int>(validIds);
//     //         var current = new HashSet<int>(user.UserUnits?.Select(x => x.UnitId) ?? Enumerable.Empty<int>());

//     //         var toAdd    = desired.Except(current).ToList();
//     //         var toRemove = current.Except(desired).ToList();

//     //         foreach (var uid in toAdd)
//     //             user.UserUnits!.Add(new UserUnitEntity { UnitId = uid, IsActive = 1 });

//     //         user.UserUnits!.RemoveWhere(uu => toRemove.Contains(uu.UnitId));
//     //     }

//     //     private async Task SyncUserRolesAsync(UserEntity user, IReadOnlyList<string> desiredRoleNames, CancellationToken ct)
//     //     {
//     //         if (desiredRoleNames.Count == 0)
//     //             return;

//     //         var companyIds = user.UserCompanies?.Select(c => c.CompanyId).Distinct().ToList() ?? new List<int>();
//     //         if (companyIds.Count == 0) return;

//     //         // Roles that match the desired names, per company
//     //         var desiredRoles = await _db.Set<UserRoleEntity>().AsNoTracking()
//     //             .Where(r => companyIds.Contains(r.CompanyId) && desiredRoleNames.Contains(r.RoleName.ToUpper()))
//     //             .Select(r => new { r.Id, NameUpper = r.RoleName.ToUpper() })
//     //             .ToListAsync(ct);

//     //         var desiredRoleIds = new HashSet<int>(desiredRoles.Select(r => r.Id));
//     //         var currentRoleIds = new HashSet<int>(user.UserRoleAllocations?.Select(a => a.UserRoleId) ?? Enumerable.Empty<int>());

//     //         // Add missing
//     //         foreach (var rid in desiredRoleIds.Except(currentRoleIds))
//     //             user.UserRoleAllocations!.Add(new UserRoleAllocationEntity { UserRoleId = rid, IsActive = 1 });

//     //         // Deactivate party-type roles that are not desired now
//     //         var partyRoleIdsAll = await _db.Set<UserRoleEntity>().AsNoTracking()
//     //             .Where(r => companyIds.Contains(r.CompanyId))
//     //             .Where(r => r.RoleName != null && (
//     //                   r.RoleName.ToUpper() == "SUPPLIER"
//     //                || r.RoleName.ToUpper() == "CUSTOMER"
//     //                || r.RoleName.ToUpper() == "AGENT"))
//     //             .Select(r => r.Id)
//     //             .ToListAsync(ct);

//     //         foreach (var alloc in user.UserRoleAllocations!.Where(a => partyRoleIdsAll.Contains(a.UserRoleId)))
//     //             alloc.IsActive = desiredRoleIds.Contains(alloc.UserRoleId) ? (byte)1 : (byte)0;
//     //     }

//     //     private async Task<string> EnsureUniqueUserNameAsync(string baseName, CancellationToken ct)
//     //     {
//     //         var users = _db.Set<UserEntity>();
//     //         var candidate = baseName;
//     //         var i = 0;
//     //         while (await users.AsNoTracking().AnyAsync(u => u.UserName == candidate, ct))
//     //             candidate = $"{baseName}{++i}";
//     //         return candidate;
//     //     }
//     // }

//     // internal static class CollectionExtensions
//     // {
//     //     public static void RemoveWhere<T>(this ICollection<T> source, Func<T, bool> predicate)
//     //     {
//     //         if (source == null || source.Count == 0) return;
//     //         var remove = source.Where(predicate).ToList();
//     //         foreach (var item in remove) source.Remove(item);
//     //     }
//     // }
// //}

