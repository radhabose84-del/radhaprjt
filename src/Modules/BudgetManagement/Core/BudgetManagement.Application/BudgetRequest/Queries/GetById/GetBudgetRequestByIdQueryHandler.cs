using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using MediatR;

namespace BudgetManagement.Application.BudgetRequest.Queries.GetById;

public class GetBudgetRequestByIdQueryHandler
    : IRequestHandler<GetBudgetRequestByIdQuery, BudgetRequestDto>
{
    private readonly IBudgetRequestQueryRepository _repo;
    private readonly ICompanyLookup _companyLookup;
    private readonly IUnitLookup _unitLookup;
    private readonly IIPAddressService _ipAddressService;

    public GetBudgetRequestByIdQueryHandler(
        IBudgetRequestQueryRepository repo,
        ICompanyLookup companyLookup,
        IUnitLookup unitLookup,
        IIPAddressService ipAddressService)
    {
        _repo = repo;
        _companyLookup = companyLookup;
        _unitLookup = unitLookup;
        _ipAddressService = ipAddressService;
    }

    public async Task<BudgetRequestDto> Handle(GetBudgetRequestByIdQuery request, CancellationToken ct)
    {
        var dto = await _repo.GetByIdAsync(request.Id, ct);
        if (dto == null) throw new Exception($"BudgetRequest {request.Id} not found.");

        // Enrich with ImageUrl if ImagePath exists
        if (!string.IsNullOrWhiteSpace(dto.ImagePath))
        {
            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyName = companies
                .FirstOrDefault(c => c.CompanyId == (_ipAddressService.GetCompanyId() ?? 0))
                ?.CompanyName ?? string.Empty;

            var unit = await _unitLookup.GetByIdAsync(_ipAddressService.GetUnitId() ?? 0, ct);
            var unitName = string.IsNullOrWhiteSpace(unit?.ShortName)
                ? unit?.UnitName ?? string.Empty
                : unit.ShortName;

            var baseDirectory = await _repo.GetBaseDirectoryAsync(ct);
            dto.ImageUrl = $"{baseDirectory}/RequestImage/{companyName}/{unitName}/{dto.ImagePath}";
        }

        return dto;
    }
}