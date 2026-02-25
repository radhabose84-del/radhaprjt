using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface IDivisionUnitGrpcClient
    {
        Task<List<DivisionUnitDto>> GetUnitsByDivisionAsync(
            int companyId,
            int divisionId,
            CancellationToken ct = default);
    }
}