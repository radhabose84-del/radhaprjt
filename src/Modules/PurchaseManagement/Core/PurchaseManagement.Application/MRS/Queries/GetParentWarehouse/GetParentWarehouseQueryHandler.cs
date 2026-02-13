using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Interfaces.External.IWarehouse;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Workflow;

namespace PurchaseManagement.Application.MRS.Queries.GetParentWarehouse
{
    public class GetParentWarehouseQueryHandler : IRequestHandler<GetParentWarehouseQuery, GetParentWarehouseDto>
    {
        private readonly IWarehouseLookup _warehouseLookup;
        private readonly IMediator _mediator;

        public GetParentWarehouseQueryHandler(IWarehouseLookup warehouseLookup, IMediator mediator)
        {
            _warehouseLookup = warehouseLookup;
            _mediator = mediator;
        }

        public async Task<GetParentWarehouseDto> Handle(GetParentWarehouseQuery request, CancellationToken cancellationToken)
        {
            var currentWarehouseList = await _warehouseLookup.GetByIdsAsync(new[] { request.WarehouseId }, cancellationToken);
            var currentWarehouse = currentWarehouseList.FirstOrDefault();

            if (currentWarehouse == null)
            {
                return new GetParentWarehouseDto
                {
                    ParentWarehouseId = 0,
                    ParentWarehouseName = "Warehouse Not Found"
                };
            }

            var parentWarehouseId = currentWarehouse.ParentWarehouseId;
            if (!parentWarehouseId.HasValue || parentWarehouseId.Value <= 0)
            {
                return new GetParentWarehouseDto
                {
                    ParentWarehouseId = 0,
                    ParentWarehouseName = "No Parent Warehouse"
                };
            }

            var parentWarehouseList = await _warehouseLookup.GetByIdsAsync(new[] { parentWarehouseId.Value }, cancellationToken);
            var parentWarehouse = parentWarehouseList.FirstOrDefault();

            var result = new GetParentWarehouseDto
            {
                ParentWarehouseId = parentWarehouse?.Id ?? 0,
                ParentWarehouseName = parentWarehouse?.WarehouseName ?? "Parent Warehouse Not Found"
            };

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetParentWarehouseQuery",
                actionName: $"ParentWarehouseId: {result.ParentWarehouseId}",
                details: $"Fetched parent warehouse for WarehouseId {request.WarehouseId}",
                module: "Warehouse"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}

