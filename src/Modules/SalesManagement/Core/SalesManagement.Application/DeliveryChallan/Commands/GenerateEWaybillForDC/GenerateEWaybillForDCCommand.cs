using Contracts.Common;
using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Commands.GenerateEWaybillForDC
{
    /// <summary>
    /// Generates an e-waybill header for a given Delivery Challan.
    /// Idempotent: if an e-waybill already exists for the DC it returns the existing id
    /// with AlreadyExisted = true rather than creating a duplicate.
    /// </summary>
    public sealed record GenerateEWaybillForDCCommand(int DeliveryChallanId)
        : IRequest<ApiResponseDTO<GenerateEWaybillResponseDto>>;
}
