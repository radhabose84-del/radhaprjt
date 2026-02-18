#nullable disable
using System.Globalization;
using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Queries.GetDutyMasterById;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;

public class GetDutyMasterByIdQueryHandler(
    IDutyMasterQueryRepository read,
    IMapper mapper,
    IMediator mediator,
    IHSNLookup hsnLookup,
    IMiscMasterCommandRepository misc)
  : IRequestHandler<GetDutyMasterByIdQuery, DutyMasterViewDto>
{
    public async Task<DutyMasterViewDto> Handle(GetDutyMasterByIdQuery r, CancellationToken ct)
    {
        // 1) Get main entity (single DB call)
        var entity = await read.GetByIdAsync(r.Id, ct);
        if (entity is null) return null;

        var dto = mapper.Map<DutyMasterViewDto>(entity);

        // 2) Batch misc IDs (single DB call)
        var miscIds = new List<int>();
        if (dto.DutyCategoryId > 0) miscIds.Add(dto.DutyCategoryId);
        if (dto.CountryOfOriginApplicability > 0) miscIds.Add(dto.CountryOfOriginApplicability);
        var miscMap = miscIds.Count == 0
            ? new Dictionary<int, MiscMaster>()
            : await misc.GetManyByIdsAsync(miscIds.Distinct(), ct); // <-- ONE DB call

        if (miscMap.TryGetValue(dto.DutyCategoryId, out var dutyCat))
            dto.DutyCategoryName = dutyCat.Description ?? dutyCat.Code ?? dutyCat.Code;

        if (miscMap.TryGetValue(dto.CountryOfOriginApplicability, out var origin))
            dto.CountryOfOriginApplicabilityName = origin.Description ?? origin.Code ?? origin.Code;

        // 3) External call may be done alone (safe to parallelize with *non-DB* work)
        if (!string.IsNullOrWhiteSpace(dto.HsnCode))
        {
            var hsn = await FetchHsnByCodeAsync(hsnLookup, dto.HsnCode!, ct);
            if (hsn is not null)
            {
                dto.Hsn = new HsnInfo
                {
                    Id = hsn.Id,
                    Code = hsn.HSNCode,
                    Description = hsn.Description,
                    Gst = hsn.GSTPercentage.ToString(CultureInfo.InvariantCulture),
                    IGst = hsn.IGSTPercentage.ToString(CultureInfo.InvariantCulture)
                };
            }
        }

        await mediator.Publish(new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: dto.Description,
            actionName: "Duty Master",
            details: $"Fetched Duty Master Id={dto.Id}, Tariff={dto.TariffNumber}.",
            module: "DutyMaster"
        ), ct);

        return dto;
    }

    private static async Task<HSNLookupDto> FetchHsnByCodeAsync(IHSNLookup hsnLookup, string code, CancellationToken ct)
    {
        var list = await hsnLookup.GetAllAsync(ct);
        return list.FirstOrDefault(x => string.Equals(x.HSNCode, code, StringComparison.OrdinalIgnoreCase));
    }
}
