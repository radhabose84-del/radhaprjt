using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Create;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Command.Create
{
    public sealed class CreateCombinePOCommandHandler
        : IRequestHandler<CreateCombinePOCommand, ApiResponseDTO<int>>
    {
        private readonly IMediator _mediator;
        private readonly IPoMethodLookup _lookup;

        public CreateCombinePOCommandHandler(IMediator mediator, IPoMethodLookup lookup)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateCombinePOCommand request, CancellationToken ct)
        {
            var r = request.Data;

            // ----------------- LOCAL PO -----------------
            if (await _lookup.IsLocalAsync(r.POMethodId, ct))
            {
                if (r.Local is null)
                {
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = "Local PO details are required for Local POMethod.",
                        Data = 0
                    };
                }

                r.Local.POMethodId = r.POMethodId;

                // CreatePurchaseOrderCommand already returns ApiResponseDTO<int>
                var response = await _mediator.Send(
                    new CreatePurchaseOrderCommand { Data = r.Local }, ct);

                // Just propagate (this includes budget-limit error)
                return response;
            }

            // ----------------- IMPORT PO -----------------
            if (await _lookup.IsImportAsync(r.POMethodId, ct))
            {
                if (r.Import is null)
                {
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = "Import PO details are required for Import POMethod.",
                        Data = 0
                    };
                }

                r.Import.POMethodId = r.POMethodId;

                // CreateImportPOCommand currently returns int
                var id = await _mediator.Send(
                    new CreateImportPOCommand { Data = r.Import }, ct);

                // Wrap int → ApiResponseDTO<int>
                return new ApiResponseDTO<int>
                {
                    IsSuccess = id > 0,
                    Message = id > 0
                        ? "Purchase order created successfully."
                        : "Purchase order was not created.",
                    Data = id
                };
            }

            // ----------------- UNSUPPORTED METHOD -----------------
            return new ApiResponseDTO<int>
            {
                IsSuccess = false,
                Message = "Unsupported POMethodId.",
                Data = 0
            };
        }
    }
}
