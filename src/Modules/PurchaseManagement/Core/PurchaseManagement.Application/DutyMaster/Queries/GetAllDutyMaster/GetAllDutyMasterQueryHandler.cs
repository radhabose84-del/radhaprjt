using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Queries.GetAllDutyMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Purchase.DutyMaster.GetAllDutyMaster
{
    public class GetDutyMastersPagedQueryHandler(
        IDutyMasterQueryRepository read,
        IMapper mapper,IMediator mediator) : IRequestHandler<GetAllDutyMasterQuery, (IReadOnlyList<DutyMasterDto> Items, int Total)>
    {
        public async Task<(IReadOnlyList<DutyMasterDto> Items, int Total)> Handle(GetAllDutyMasterQuery r, CancellationToken ct)
        {
             var (entities, total) = await read.GetAllAsync(r.PageNumber, r.PageSize, r.Search, ct);
            var items = entities.Select(mapper.Map<DutyMasterDto>).ToList();

            // publish read audit (list)
            var audit = new AuditLogsDomainEvent(
                actionDetail: "List",
                actionCode: $"Page={r.PageNumber},Size={r.PageSize}",
                actionName: "Duty Master",
                details: $"Listed Duty Masters: total={total}, returned={items.Count}, search='{r.Search ?? ""}'.",
                module: "DutyMaster"
            );
            await mediator.Publish(audit, ct);

            return (items, total);
        }
    }
}
