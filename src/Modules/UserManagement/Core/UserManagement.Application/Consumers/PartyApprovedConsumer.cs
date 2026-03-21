using Contracts.Events.Users;
using Contracts.Interfaces;
using UserManagement.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

using UserEntity = UserManagement.Domain.Entities.User;
using UserRoleAllocationEntity = UserManagement.Domain.Entities.UserRoleAllocation;
using UserGroupEntity = UserManagement.Domain.Entities.UserGroup;
using UserCompanyEntity = UserManagement.Domain.Entities.UserCompany;
using UserUnitEntity = UserManagement.Domain.Entities.UserUnit;
using CompanyEntity = UserManagement.Domain.Entities.Company;
using UnitEntity = UserManagement.Domain.Entities.Unit;

namespace UserManagement.Application.Consumers
{
    public class PartyApprovedConsumer : IConsumer<PartyApprovedIntegrationEvent>
    {
        private readonly DbContext _db;
        private readonly IIPAddressService _iPAddressService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PartyApprovedConsumer> _logger;

        public PartyApprovedConsumer(DbContext db, IIPAddressService iPAddressService, IConfiguration configuration, ILogger<PartyApprovedConsumer> logger)
        {
            _db = db;
            _iPAddressService = iPAddressService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PartyApprovedIntegrationEvent> context)
        {
            var ct = context.CancellationToken;
            var m = context.Message;

            var users = _db.Set<User>();

            // Idempotency: if a user already exists for this party, stop.
            if (await _db.Set<User>().AsNoTracking().AnyAsync(u => u.PartyId == m.PartyId, ct))
                return;

            // Username
            var baseUserName = !string.IsNullOrWhiteSpace(m.Email)
                ? m.Email!.Trim().ToLowerInvariant()
                : !string.IsNullOrWhiteSpace(m.PartyCode) ? m.PartyCode!.Trim()
                : $"party{m.PartyId}";
            var userName = await EnsureUniqueUserNameAsync(baseUserName);

            // resolve audit values (payload -> headers -> defaults)
            var (createdBy, createdByName, createdIp, createdAt) = ResolveAudit(context, m, _iPAddressService);

            // Build user
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = m.PartyName,
                LastName = m.PartyLastName,
                UserName = userName,
                IsFirstTimeUser = UserManagement.Domain.Enums.Common.Enums.FirstTimeUserStatus.Yes,
                Mobile = m.Mobile,
                EmailId = m.Email,
                IsLocked = 0,
                PartyId = m.PartyId,
                DepartmentId = 1,
                CreatedBy = createdBy,
                CreatedByName = createdByName,
                CreatedIP = createdIp,
                CreatedAt = createdAt,
                IsActive = UserManagement.Domain.Enums.Common.Enums.Status.Active,
                IsDeleted = UserManagement.Domain.Enums.Common.Enums.IsDelete.NotDeleted
            };

            // === MULTI COMPANIES & UNITS (FK-safe) ===
            var requestedCompanyIds = (m.CompanyIds ?? Array.Empty<int>()).Where(i => i > 0).Distinct().ToList();
            var requestedUnitIds = (m.UnitIds ?? Array.Empty<int>()).Where(i => i > 0).Distinct().ToList();

            // Validate companies
            var validCompanyIds = requestedCompanyIds.Count == 0
                ? new List<int>()
                : await _db.Set<CompanyEntity>()
                    .AsNoTracking()
                    .Where(c => requestedCompanyIds.Contains(c.Id))
                    .Select(c => c.Id)
                    .ToListAsync(ct);

            if (m.DefaultCompanyId.HasValue &&
                !validCompanyIds.Contains(m.DefaultCompanyId.Value) &&
                await _db.Set<CompanyEntity>().AsNoTracking().AnyAsync(c => c.Id == m.DefaultCompanyId.Value, ct))
            {
                validCompanyIds.Add(m.DefaultCompanyId.Value);
            }

            user.UserCompanies = validCompanyIds
                .Distinct()
                .Select(id => new UserCompanyEntity { CompanyId = id, IsActive = 1 })
                .ToList();

            // Validate units
            var validUnitIds = requestedUnitIds.Count == 0
                ? new List<int>()
                : await _db.Set<UnitEntity>()
                    .AsNoTracking()
                    .Where(u => requestedUnitIds.Contains(u.Id))
                    .Select(u => u.Id)
                    .ToListAsync(ct);

            if (m.DefaultUnitId.HasValue &&
                !validUnitIds.Contains(m.DefaultUnitId.Value) &&
                await _db.Set<UnitEntity>().AsNoTracking().AnyAsync(u => u.Id == m.DefaultUnitId.Value, ct))
            {
                validUnitIds.Add(m.DefaultUnitId.Value);
            }

            user.UserUnits = validUnitIds
                .Distinct()
                .Select(id => new UserUnitEntity { UnitId = id, IsActive = 1 })
                .ToList();

            // ===== Roles (MANDATORY, never create) =====
            var desiredRoleNames = MapPartyTypesToRoleNames(m.PartyTypeCodes ?? Array.Empty<string>());

            var ensuredRoleIds = new List<int>();
            if (desiredRoleNames.Count > 0 && user.UserCompanies.Any())
            {
                var companyIds = user.UserCompanies.Select(c => c.CompanyId).Distinct().ToList();
                ensuredRoleIds = await GetExistingRolesForCompaniesOrThrowAsync(companyIds, desiredRoleNames, ct);

                user.UserRoleAllocations ??= new List<UserRoleAllocationEntity>();
                foreach (var roleId in ensuredRoleIds)
                {
                    if (!user.UserRoleAllocations.Any(a => a.UserRoleId == roleId))
                        user.UserRoleAllocations.Add(new UserRoleAllocationEntity { UserRoleId = roleId, IsActive = 1 });
                }
            }

            if (m.DefaultRoleId.HasValue)
            {
                user.UserRoleAllocations ??= new List<UserRoleAllocationEntity>();
                if (!user.UserRoleAllocations.Any(a => a.UserRoleId == m.DefaultRoleId.Value))
                    user.UserRoleAllocations.Add(new UserRoleAllocationEntity { UserRoleId = m.DefaultRoleId.Value, IsActive = 1 });
            }

            // Password (BCrypt via domain method)
            var tempPassword = $"P@rty{Guid.NewGuid():N}"[..12];
            user.SetPassword(tempPassword);

            user.UserType = await ResolveUserTypeInternalAsync(ct);
            user.EntityId = await ResolveEntityIdFromCompaniesAsync(user.UserCompanies, ct);
            user.UserGroupId = await ResolveDefaultUserGroupIdAsync(ct);

            // Save
            users.Add(user);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("User created for PartyId={PartyId}, UserName={UserName}", m.PartyId, userName);

            // -------- FIRST-TIME EMAIL (SMTP) --------
            if (!string.IsNullOrWhiteSpace(user.EmailId))
            {
                try
                {
                    var providerKey = user.EmailId.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
                        ? "Gmail" : "Zimbra";

                    var smtpHost = _configuration[$"EmailSettings:Providers:{providerKey}:Host"] ?? "smtp.gmail.com";
                    var smtpPort = int.TryParse(_configuration[$"EmailSettings:Providers:{providerKey}:Port"], out var p) ? p : 587;
                    var smtpSsl = bool.TryParse(_configuration[$"EmailSettings:Providers:{providerKey}:EnableSsl"], out var s) && s;
                    var smtpUser = _configuration[$"EmailSettings:Providers:{providerKey}:UserName"] ?? "";
                    var smtpPass = _configuration[$"EmailSettings:Providers:{providerKey}:Password"] ?? "";

                    var subject = "Your portal account is ready";
                    var html = $@"
                        <p>Dear {(string.IsNullOrWhiteSpace(user.FirstName) ? "User" : user.FirstName)},</p>
                        <p>Your portal access has been created.</p>
                        <p><b>Username:</b> {user.UserName}<br/>
                           <b>Temporary password:</b> {tempPassword}</p>
                        <p>Please sign in and change your password immediately.</p>
                        <p>Thanks,<br/>Support Team</p>";

                    using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                    {
                        Credentials = new NetworkCredential(smtpUser, smtpPass),
                        EnableSsl = smtpSsl
                    };

                    using var mailMessage = new MailMessage
                    {
                        From = new MailAddress(smtpUser),
                        Subject = subject,
                        Body = html,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(user.EmailId);

                    await smtpClient.SendMailAsync(mailMessage);
                    _logger.LogInformation("Welcome email sent to {Email} for PartyId {PartyId}", user.EmailId, m.PartyId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending welcome email to {Email} for PartyId {PartyId}", user.EmailId, m.PartyId);
                }
            }
        }

        private async Task<string> EnsureUniqueUserNameAsync(string baseName)
        {
            var users = _db.Set<User>();
            var candidate = baseName; var i = 0;

            while (await users.AsNoTracking().AnyAsync(u => u.UserName == candidate))
                candidate = $"{baseName}{++i}";

            return candidate;
        }

        private static List<string> MapPartyTypesToRoleNames(IReadOnlyList<string> partyTypeCodes)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var code in partyTypeCodes ?? Array.Empty<string>())
            {
                var c = (code ?? "").Trim().ToUpperInvariant();
                if (c is "SUPPLIER" or "CUSTOMER" or "AGENT") set.Add(c);
            }
            return set.ToList();
        }

        private async Task<List<int>> GetExistingRolesForCompaniesOrThrowAsync(
            IReadOnlyList<int> companyIds,
            IReadOnlyList<string> roleNames,
            CancellationToken ct)
        {
            var roleIds = new List<int>();

            if (companyIds == null || companyIds.Count == 0 || roleNames == null || roleNames.Count == 0)
                return roleIds;

            var existing = await _db.Set<UserManagement.Domain.Entities.UserRole>()
                .AsNoTracking()
                .Where(r => companyIds.Contains(r.CompanyId) && roleNames.Contains(r.RoleName))
                .Select(r => new { r.Id, r.RoleName, r.CompanyId })
                .ToListAsync(ct);

            var existingLookup = existing
                .GroupBy(e => (e.CompanyId, e.RoleName))
                .ToDictionary(g => g.Key, g => g.Select(x => x.Id).First());

            var missing = new List<(int CompanyId, string RoleName)>();

            foreach (var companyId in companyIds)
            {
                foreach (var roleName in roleNames)
                {
                    if (existingLookup.TryGetValue((companyId, roleName), out var id))
                        roleIds.Add(id);
                    else
                        missing.Add((companyId, roleName));
                }
            }

            if (missing.Count > 0)
            {
                var msg = "Required roles are missing in AppSecurity.UserRole: " +
                          string.Join(", ", missing.Select(m => $"(CompanyId={m.CompanyId}, RoleName='{m.RoleName}')"));
                _logger.LogWarning(msg);
                // Don't throw — just skip role assignment for missing roles
            }

            return roleIds.Distinct().ToList();
        }

        private async Task<int> ResolveUserTypeInternalAsync(CancellationToken ct)
        {
            const string miscTypeCode = Domain.Enums.Common.MiscEnumEntity.UserType.MiscTypeCode;
            const string code = Domain.Enums.Common.MiscEnumEntity.UserType.External;

            var miscTypeId = await _db.Set<UserManagement.Domain.Entities.MiscTypeMaster>()
                .AsNoTracking()
                .Where(t => t.MiscTypeCode == miscTypeCode)
                .Select(t => (int?)t.Id)
                .FirstOrDefaultAsync(ct);

            if (miscTypeId is null)
                return 1;

            var id = await _db.Set<UserManagement.Domain.Entities.MiscMaster>()
                .AsNoTracking()
                .Where(x => x.MiscTypeId == miscTypeId.Value
                        && x.Code == code
                        && x.IsActive == Domain.Enums.Common.Enums.Status.Active
                        && x.IsDeleted == 0)
                .OrderBy(x => x.Id)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(ct);

            return id ?? 1;
        }

        private async Task<int?> ResolveEntityIdFromCompaniesAsync(IList<UserCompanyEntity>? userCompanies, CancellationToken ct)
        {
            if (userCompanies == null || userCompanies.Count == 0)
                return null;

            var firstCompanyId = userCompanies.Select(c => c.CompanyId).FirstOrDefault();
            if (firstCompanyId <= 0)
                return null;

            var entityId = await _db.Set<CompanyEntity>()
                .AsNoTracking()
                .Where(c => c.Id == firstCompanyId)
                .Select(c => (int?)c.EntityId)
                .FirstOrDefaultAsync(ct);

            return entityId;
        }

        private async Task<int?> ResolveDefaultUserGroupIdAsync(CancellationToken ct)
        {
            var idByCode = await _db.Set<UserGroupEntity>()
                .AsNoTracking()
                .Where(g => g.IsActive == Domain.Enums.Common.Enums.Status.Active)
                .Where(g => g.GroupCode == "USER")
                .OrderBy(g => g.Id)
                .Select(g => (int?)g.Id)
                .FirstOrDefaultAsync(ct);

            if (idByCode.HasValue)
                return idByCode;

            var idByName = await _db.Set<UserGroupEntity>()
                .AsNoTracking()
                .Where(g => g.IsActive == Domain.Enums.Common.Enums.Status.Active)
                .Where(g => g.GroupName != null && g.GroupName.Contains("User"))
                .OrderBy(g => g.Id)
                .Select(g => (int?)g.Id)
                .FirstOrDefaultAsync(ct);

            return idByName;
        }

        private static (int createdBy, string createdByName, string createdIp, DateTime createdAt)
            ResolveAudit(ConsumeContext<PartyApprovedIntegrationEvent> ctx,
                        PartyApprovedIntegrationEvent m,
                        IIPAddressService ipSvc)
        {
            var by = m.CreatedBy;
            var byNm = m.CreatedByName;
            var ip = m.CreatedIp;
            var at = m.CreatedAt == default ? DateTime.UtcNow : m.CreatedAt;

            if (by == 0 && ctx.Headers.TryGetHeader("user-id", out var uidObj)
                && int.TryParse(uidObj?.ToString(), out var uidParsed))
                by = uidParsed;

            if (string.IsNullOrWhiteSpace(byNm) &&
                ctx.Headers.TryGetHeader("user-name", out var unameObj))
                byNm = unameObj?.ToString();

            if (string.IsNullOrWhiteSpace(ip) &&
                ctx.Headers.TryGetHeader("ip", out var ipObj))
                ip = ipObj?.ToString();

            if (by == 0) by = ipSvc.GetUserId();
            if (string.IsNullOrWhiteSpace(byNm)) byNm = ipSvc.GetUserName() ?? "System";
            if (string.IsNullOrWhiteSpace(ip)) ip = ipSvc.GetSystemIPAddress() ?? "0.0.0.0";

            return (by, byNm!, ip!, at);
        }
    }
}
