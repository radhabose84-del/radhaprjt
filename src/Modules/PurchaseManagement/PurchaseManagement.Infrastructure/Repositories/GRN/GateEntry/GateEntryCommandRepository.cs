#nullable disable
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.GRN.GateEntry
{
    public class GateEntryCommandRepository : IGateEntryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;
        public GateEntryCommandRepository(ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
        }
        public async Task<int> CreateAsync(GateEntryHeader gateEntryHeader)
        {
            // Add main PartyMaster
            await _applicationDbContext.GateEntryHeader.AddAsync(gateEntryHeader);

            // EF will automatically save non-null child collections
            await _applicationDbContext.SaveChangesAsync();

            return gateEntryHeader.Id; 
        }

        public Task<bool> DeleteAsync(int Id, GateEntryHeader gateEntryHeader)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(int Id, GateEntryHeader gateEntryHeader)
        {
            throw new NotImplementedException();
        }
        public async Task<string> GenerateNextCodeAsync(CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitCode = unitId > 0 ? unitId.ToString() : "NA";
            var prefix = $"GE-{unitCode}-";

            var recent = await _applicationDbContext.GateEntryHeader.AsNoTracking()
                .Where(r => r.GateEntryNo.StartsWith(prefix))
                .OrderByDescending(r => r.Id)
                .Select(r => r.GateEntryNo)
                .Take(100)
                .ToListAsync(ct);

            var max = 0;
            foreach (var code in recent)
            {
                var suffix = code.Substring(prefix.Length);
                if (int.TryParse(suffix, out var n) && n > max) max = n;
            }

            return $"{prefix}{(max + 1):D2}";
        }
    }
}