using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.MRS.Queries.GetMrsEntryById
{
    public class GetMrsEntryByIdQueryHandler : IRequestHandler<GetMrsEntryByIdQuery, GetMrsEntryByIdDto>
    {
        private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IItemPurchaseToleranceLookup _itemPurchaseToleranceLookup;

        public GetMrsEntryByIdQueryHandler(
            IMrsEntryQueryRepository iMrsEntryQueryRepository,
            IMapper mapper,
            IMediator mediator,
            IDepartmentLookup departmentLookup,
            IUOMLookup uomLookup,
            IItemPurchaseToleranceLookup itemPurchaseToleranceLookup)
        {
            _iMrsEntryQueryRepository = iMrsEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;
            _uomLookup = uomLookup;
            _itemPurchaseToleranceLookup = itemPurchaseToleranceLookup;
        }

        public async Task<GetMrsEntryByIdDto> Handle(GetMrsEntryByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Fetch Header + Details
            var dto = await _iMrsEntryQueryRepository.GetMrsDetailsById(request.Id);
            if (dto == null)
                throw new KeyNotFoundException("Mrs not found");

            // 2. Collect IDs
            var uomIds = dto.MrsDetails.Select(x => x.UomId).Distinct().ToList();
            var itemIds = dto.MrsDetails.Select(x => x.ItemId).Distinct().ToList();

            // 3. Fire parallel lookup calls
            var departmentTask = _departmentLookup.GetAllDepartmentAsync();
            var uomTask = _uomLookup.GetByIdsAsync(uomIds, cancellationToken);
            var itemTask = _itemPurchaseToleranceLookup.GetByIdsAsync(itemIds, cancellationToken);

            // 4. Await all together
            await Task.WhenAll(departmentTask, uomTask, itemTask);

            // 5. Prepare lookup dictionaries
            var departmentById = departmentTask.Result.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);
            var uomById = uomTask.Result.Where(u => u != null).ToDictionary(u => u.Id, u => u);
            var itemById = itemTask.Result.ToDictionary(i => i.ItemId, i => i);

            // 6. Enrich Header
            dto.DepartmentName = departmentById.GetValueOrDefault(dto.DepartmentId, "NA");
            dto.SubDepartmentName = departmentById.GetValueOrDefault(dto.SubDepartmentId, "NA");

            // 7. Enrich Details
            foreach (var line in dto.MrsDetails)
            {
                // Item Info
                if (itemById.TryGetValue(line.ItemId, out var item))
                {
                    line.ItemCode = item.ItemCode ?? "NA";
                    line.ItemName = item.ItemName ?? "NA";
                }
                else
                {
                    line.ItemCode = "NA";
                    line.ItemName = "NA";
                }

                // UOM Info
                if (uomById.TryGetValue(line.UomId, out var uom))
                {
                    line.UomName = uom.UOMName ?? "NA";
                }
                else
                {
                    line.UomName = "NA";
                }
            }

            // 8. Audit log
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMrsEntryByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Mrs details {dto.Id} fetched.",
                module: "MrsEntry"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            return dto;
        }
    }
}