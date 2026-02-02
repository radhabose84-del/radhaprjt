
using PurchaseManagement.Application.Port.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IPortMaster;
public interface IPortMasterQueryRepository
{
    Task<PortMasterDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(IReadOnlyList<PortMasterDto> Items, int Total)> GetAllAsync(int page, int size, string? search, int? countryId,int? portTypeId, CancellationToken ct);
    Task<IReadOnlyList<PortLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
}

public sealed class PortLookupDto { public int Id { get; set; } public string portCode { get; set; } = default! ;  public string portname { get; set; } = default! ; }
