using WarehouseManagement.Application.Common.Interfaces.IBinMaster;

namespace WarehouseManagement.Infrastructure.Repositories.BinMaster
{
    public class BinCodeGenerator  : IBinCodeGenerator
{
    
    private readonly IBinMasterQueryRepository _binMasterQueryRepository;

    public BinCodeGenerator(IBinMasterQueryRepository  binMasterQueryRepository )
    {
        _binMasterQueryRepository = binMasterQueryRepository;
    }

        public async  Task<string> GenerateAsync(int warehouseId, int? rackId, CancellationToken ct = default)
        {
            // Pattern: BIN-<WH>-R<rack>-<N>
                var rackPart = rackId.HasValue ? $"-R{rackId.Value}" : string.Empty;
                var prefix = $"BIN-{warehouseId}{rackPart}-";

                // 1) Get all existing codes with this prefix
                var existingCodes = await _binMasterQueryRepository.GetBinCodesByPrefixAsync(prefix, ct);

                // 2) Extract sequence numbers
                var maxSeq = 0;
                foreach (var code in existingCodes)
                {
                    var lastPart = code.Replace(prefix, "");
                    if (int.TryParse(lastPart, out var n))
                    {
                        if (n > maxSeq) maxSeq = n;
                    }
                }

                // 3) Next sequence
                var nextSeq = maxSeq + 1;

                return $"{prefix}{nextSeq}";
        
        }
    }
}