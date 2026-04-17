using Contracts.Dtos.Common;

namespace Contracts.Interfaces.Gate
{
    public interface IGatePassDocResolver
    {
        string DocumentType { get; }

        Task<IReadOnlyList<GatePassDocSummaryDto>> GetSummariesAsync(IEnumerable<int> docIds, CancellationToken ct = default);
    }
}
