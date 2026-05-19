using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PurchaseManagement.Infrastructure.Data;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.EntityFrameworkCore.Storage;

namespace PurchaseManagement.Infrastructure.Repositories.Quotation.RfqEntry
{
    public class RfqCommandRepository :  IRfqCommandRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IIPAddressService _ip;
        private readonly IUnitLookup _unitLookup;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public RfqCommandRepository(ApplicationDbContext db, IIPAddressService ip, IUnitLookup unitLookup,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _db = db;
            _ip = ip;
            _unitLookup = unitLookup;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(RfqMaster rfq, int transactionTypeId, CancellationToken ct = default)
        {
            rfq.RfqStatusId = await GetStatusIdByCodeAsync("SUBMIT", ct);

            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                await _db.Rfqs.AddAsync(rfq, ct);
                await _db.SaveChangesAsync(ct);

                var dbConnection = _db.Database.GetDbConnection();
                var dbTransaction = transaction.GetDbTransaction();
                await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                await transaction.CommitAsync(ct);
                return rfq.Id;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public Task<RfqMaster?> GetAggregateTrackingAsync(int id, CancellationToken ct = default) =>
            _db.Rfqs
               .Include(r => r.Items)
               .Include(r => r.Suppliers)
               .FirstOrDefaultAsync(r => r.Id == id, ct);

        public async Task<bool> IsDraftAsync(int id, CancellationToken ct = default)
        {
            var draftId = await GetStatusIdByCodeAsync("DRAFT", ct);
            return await _db.Rfqs
                .Where(r => r.Id == id)
                .Select(r => r.RfqStatusId == draftId)
                .SingleAsync(ct);
        }

        public async Task UpdateAsync(
            int id,
            RfqMaster headerAfter,
            List<RfqItem> desiredItems,
            List<RfqSupplier> desiredSuppliers,
            CancellationToken ct = default)
        {
            // Execution strategy + transaction (resilient)
            var strategy = _db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                var rfq = await _db.Set<RfqMaster>()
                    .FirstOrDefaultAsync(x => x.Id == id, ct)
                    ?? throw new KeyNotFoundException("RFQ not found.");

                // ---- Header only (no line touching here) ----
                var fromIndent = await IsFromIndentAsync(headerAfter.InitiationTypeId??0, ct);
                rfq.UnitId           = headerAfter.UnitId;
                rfq.InitiationTypeId = headerAfter.InitiationTypeId;
                rfq.IndentId         = headerAfter.IndentId > 0 ? headerAfter.IndentId : null;
                rfq.RfqStatusId      = headerAfter.RfqStatusId; // already resolved in handler
                rfq.IsActive         = headerAfter.IsActive;
                rfq.ModifiedDate     = DateTimeOffset.UtcNow;

                // ---- Lines: hard replace (DELETE ALL → INSERT fresh) ----
                // Prefer bulk delete when supported (EF Core 7+ / provider supports ExecuteDelete)
                var itemsQuery = _db.Set<RfqItem>().Where(i => i.RfqId == id);
                var supsQuery  = _db.Set<RfqSupplier>().Where(s => s.RfqId == id);

                // Try server-side bulk delete; if provider doesn't support, fallback to load+RemoveRange
                try
                {
                    await itemsQuery.ExecuteDeleteAsync(ct);
                    await supsQuery.ExecuteDeleteAsync(ct);
                }
                catch (NotSupportedException)
                {
                    var existingItems = await itemsQuery.ToListAsync(ct);
                    var existingSups  = await supsQuery.ToListAsync(ct);
                    _db.RemoveRange(existingItems);
                    _db.RemoveRange(existingSups);
                }

                if (desiredItems is not null && desiredItems.Count > 0)
                {
                    foreach (var d in desiredItems)
                    {
                        _db.Add(new RfqItem
                        {
                            RfqId    = rfq.Id,
                            ItemId   = d.ItemId,
                            HsnId    = d.HsnId,
                            Quantity = d.Quantity,
                            UomId    = d.UomId
                        });
                    }
                }

                if (desiredSuppliers is not null && desiredSuppliers.Count > 0)
                {
                    foreach (var s in desiredSuppliers)
                    {
                        var sup = new RfqSupplier
                        {
                            RfqId      = rfq.Id,
                            SupplierId = s.SupplierId,
                            Name       = s.Name ?? string.Empty,
                            Mobile     = s.Mobile,
                            GSTNumber  = s.GSTNumber
                        };

                        // If Email is a ValueObject in your domain:
                        // if (!string.IsNullOrWhiteSpace(s.Email?.Value)) sup.Email = new EmailAddress(s.Email.Value);
                        // Else (string):
                        sup.Email = s.Email;

                        _db.Add(sup);
                    }
                }

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });
        }
        
        /* public async Task<string> GenerateNextCodeAsync( DateTimeOffset rfqDate,CancellationToken ct = default)
        {
            var unitId = _ip.GetUnitId() ?? 0;
              string unitCode;
            var Units = await _unitLookup.GetAllUnitAsync();
            var UnitLookup = Units.ToDictionary(d => d.UnitId, d => d.ShortName);

            if (UnitLookup.TryGetValue(unitId, out var ShortName))
            {
                unitCode = ShortName;
            }
            else
            {
                throw new Exception("Invalid Unit Id. Failed to generate indent number.");
            }

            var prefix = $"RFQ-{unitCode}-";
            var (fyStart, fyEndEx) = GetFyRange(rfqDate.UtcDateTime, 4);

            var recent = await _db.Rfqs.AsNoTracking()
                .Where(r => r.RfqCode.StartsWith(prefix))
                .OrderByDescending(r => r.Id)
                .Select(r => r.RfqCode)
                .Take(100)
                .ToListAsync(ct);

            var max = 0;
            foreach (var code in recent)
            {
                var suffix = code.Substring(prefix.Length);
                if (int.TryParse(suffix, out var n) && n > max) max = n;
            }

            return $"{prefix}{(max + 1):D2}";
        } */
        private static (DateTimeOffset start, DateTimeOffset endExclusive) GetFyRange(DateTimeOffset poDate, int startMonth)
        {
            var y = poDate.Month >= startMonth ? poDate.Year : poDate.Year - 1;
            var start = new DateTimeOffset(new DateTime(y, startMonth, 1), TimeSpan.Zero);
            return (start, start.AddYears(1));
        }

        private Task<bool> IsFromIndentAsync(int initiationTypeId, CancellationToken ct) =>
            _db.MiscMaster
               .Where(m => m.Id == initiationTypeId)
               .Select(m => m.Code == "FROM_INDENT")
               .SingleAsync(ct);
        
        public async Task UpdateDraftPartialAsync(
            int id,
            RfqMaster headerAfter,
            List<RfqItem>? desiredItems,
            List<RfqSupplier>? desiredSuppliers,
            CancellationToken ct = default)
        {
            var rfq = await _db.Rfqs
                .Include(r => r.Items)
                .Include(r => r.Suppliers)
                .FirstOrDefaultAsync(r => r.Id == id, ct)
                ?? throw new KeyNotFoundException("RFQ not found.");

            var draftId = await GetStatusIdByCodeAsync("DRAFT", ct);
            if (rfq.RfqStatusId != draftId)
                throw new InvalidOperationException("Only Draft RFQ can be edited.");

            // ----- HEADER PATCH -----
            if (headerAfter.UnitId > 0)
                rfq.UnitId = headerAfter.UnitId;

            if (headerAfter.InitiationTypeId > 0)
                rfq.InitiationTypeId = headerAfter.InitiationTypeId;

            if (headerAfter.IndentId.HasValue && headerAfter.IndentId > 0)
                rfq.IndentId = headerAfter.IndentId;

            rfq.ModifiedDate = DateTimeOffset.UtcNow;

            // ----- ITEMS PATCH (update + add, no delete) -----
            if (desiredItems is not null)
            {
                foreach (var d in desiredItems)
                {
                    if (d.Id > 0)
                    {
                        // update existing item
                        var existing = rfq.Items.FirstOrDefault(x => x.Id == d.Id);
                        if (existing is not null)
                        {
                            existing.ItemId   = d.ItemId;
                            existing.HsnId    = d.HsnId;
                            existing.Quantity = d.Quantity;
                            existing.UomId    = d.UomId;
                        }
                        else
                        {
                            // safety net: id not found → treat as new
                            rfq.Items.Add(new RfqItem
                            {
                                RfqId    = rfq.Id,
                                ItemId   = d.ItemId,
                                HsnId    = d.HsnId,
                                Quantity = d.Quantity,
                                UomId    = d.UomId
                            });
                        }
                    }
                    else
                    {
                        // new line in draft
                        rfq.Items.Add(new RfqItem
                        {
                            RfqId    = rfq.Id,
                            ItemId   = d.ItemId,
                            HsnId    = d.HsnId,
                            Quantity = d.Quantity,
                            UomId    = d.UomId
                        });
                    }
                }             
            }
            // ----- SUPPLIERS PATCH (update + add, no delete) -----
            if (desiredSuppliers is not null)
            {
                foreach (var d in desiredSuppliers)
                {
                    if (d.Id > 0)
                    {
                        var existing = rfq.Suppliers.FirstOrDefault(x => x.Id == d.Id);
                        if (existing is not null)
                        {
                            existing.SupplierId = d.SupplierId;
                            existing.Name       = d.Name ?? string.Empty;
                            existing.Email      = d.Email;
                            existing.Mobile     = d.Mobile;
                            existing.GSTNumber  = d.GSTNumber;
                        }
                        else
                        {
                            rfq.Suppliers.Add(new RfqSupplier
                            {
                                RfqId      = rfq.Id,
                                SupplierId = d.SupplierId,
                                Name       = d.Name ?? string.Empty,
                                Email      = d.Email,
                                Mobile     = d.Mobile,
                                GSTNumber  = d.GSTNumber
                            });
                        }
                    }
                    else
                    {
                        rfq.Suppliers.Add(new RfqSupplier
                        {
                            RfqId      = rfq.Id,
                            SupplierId = d.SupplierId,
                            Name       = d.Name ?? string.Empty,
                            Email      = d.Email,
                            Mobile     = d.Mobile,
                            GSTNumber  = d.GSTNumber
                        });
                    }
                }                
            }
            await _db.SaveChangesAsync(ct);
        }


        public Task<int> GetStatusIdByCodeAsync(string code, CancellationToken ct = default)
      => GetStatusIdByCodeCoreAsync(code, ct);

        // private core helper you can call internally
        private async Task<int> GetStatusIdByCodeCoreAsync(string code, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Status code is required.", nameof(code));
            var id = await _db.MiscMaster
                .AsNoTracking()
                .Where(m => m.Code != null && m.Code.ToUpper() == code.ToUpper())
                .Select(m => (int?)m.Id)
                .FirstOrDefaultAsync(ct);

            if (id is null)
                throw new InvalidOperationException($"RFQ status code '{code}' not found.");

            return id.Value;
        }
        public async Task<bool> RollbackStatusAsync(int id, CancellationToken ct = default)
        {
            var rfq = await _db.Rfqs
                .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted == Domain.Common.BaseEntity.IsDelete.NotDeleted, ct);

            if (rfq is null)
                return false;

            var draftId = await GetStatusIdByCodeAsync("DRAFT", ct);
            rfq.RfqStatusId = draftId;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<List<SupplierContacts>> GetSupplierContactsAsync(int rfqId, CancellationToken ct)
        => await _db.RfqSuppliers
            .AsNoTracking()
            .Where(s => s.RfqId == rfqId)
            .Select(s => new SupplierContacts(
                s.Id,
                s.Email != null ? s.Email.Value : null,
                s.Mobile))
            .ToListAsync(ct);

        public async Task<string?> SoftDeleteAttachmentAsync(int rfqId, int attachmentId, CancellationToken ct = default)
        {
            var attachment = await _db.RfqAttachments
                .FirstOrDefaultAsync(a =>
                    a.Id == attachmentId &&
                    a.RfqId == rfqId &&
                    a.IsDeleted == Domain.Common.BaseEntity.IsDelete.NotDeleted, ct);

            if (attachment == null)
                return null;

            attachment.IsDeleted = Domain.Common.BaseEntity.IsDelete.Deleted;
            attachment.ModifiedBy = _ip.GetUserId();
            attachment.ModifiedByName = _ip.GetUserName();
            attachment.ModifiedIP = _ip.GetSystemIPAddress();
            attachment.ModifiedDate = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
            return attachment.FilePath;
        }
    }
}
