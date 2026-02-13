using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Interfaces.Lookups.Warehouse;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetParentWarehouse
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
             // 1️⃣ Get the current warehouse by Id (example: 59)
            var currentWarehouse = (await _warehouseLookup.GetByIdsAsync(new[] { request.WarehouseId }, cancellationToken)).FirstOrDefault();

            if (currentWarehouse == null)
            {
                return new GetParentWarehouseDto
                {
                    ParentWarehouseId = 0,
                    ParentWarehouseName = "Warehouse Not Found"
                };
            }

            // 2️⃣ If no parent warehouse exists
            var parentWarehouseId = currentWarehouse.ParentWarehouseId.GetValueOrDefault();
            if (parentWarehouseId <= 0)
            {
                return new GetParentWarehouseDto
                {
                    ParentWarehouseId = 0,
                    ParentWarehouseName = "No Parent Warehouse"
                };
            }

            // 3️⃣ Fetch the actual parent warehouse (example: Id = 55)
            var parentWarehouse = (await _warehouseLookup.GetByIdsAsync(new[] { parentWarehouseId }, cancellationToken))
                .FirstOrDefault();

            // 4️⃣ Map the parent warehouse details
            var result = new GetParentWarehouseDto
            {
                ParentWarehouseId = parentWarehouse?.Id ?? 0,
                ParentWarehouseName = parentWarehouse?.WarehouseName ?? "Parent Warehouse Not Found"
            };

            // 5️⃣ Optional audit event for tracking
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
