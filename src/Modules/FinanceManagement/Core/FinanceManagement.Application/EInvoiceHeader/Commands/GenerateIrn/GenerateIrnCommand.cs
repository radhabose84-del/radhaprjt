using Contracts.Common;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.GenerateIrn
{
    public sealed record GenerateIrnCommand(int EInvoiceHeaderId)
        : IRequest<ApiResponseDTO<NicIrnResultDto>>;
}
