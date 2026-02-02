using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Infrastructure.Repositories.PoMethodLookup
{
    public sealed class PoMethodLookup : IPoMethodLookup
    {
        private readonly IMiscMasterQueryRepository _misc;

        public PoMethodLookup(IMiscMasterQueryRepository misc)
        { 
            _misc = misc; 
        }

        private sealed class PoIds 
        { 
            public required int LocalId; 
            public required int ImportId; 
        }

        // Fetch fresh, no caching
        private async Task<PoIds> LoadAsync(CancellationToken ct)
        {
            // Expect your repo method to be case-insensitive (as updated earlier)
            var local  = await _misc.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Local);
            var import = await _misc.GetMiscMasterByName(MiscEnumEntity.POMethod, MiscEnumEntity.Import);

            if (local is null || local.Id <= 0 || import is null || import.Id <= 0)
                throw new InvalidOperationException("POMethod misc not configured (Local/Import not found or inactive).");

            return new PoIds { LocalId = local.Id, ImportId = import.Id };
        }

        public async Task<int> GetLocalIdAsync(CancellationToken ct)
            => (await LoadAsync(ct)).LocalId;

        public async Task<int> GetImportIdAsync(CancellationToken ct)
            => (await LoadAsync(ct)).ImportId;

        public async Task<bool> IsLocalAsync(int id, CancellationToken ct)
            => id == (await LoadAsync(ct)).LocalId;

        public async Task<bool> IsImportAsync(int id, CancellationToken ct)
            => id == (await LoadAsync(ct)).ImportId;

        public async Task<bool> IsValidAsync(int id, CancellationToken ct)
        {
            var ids = await LoadAsync(ct);
            return id == ids.LocalId || id == ids.ImportId;
        }
    }
}
