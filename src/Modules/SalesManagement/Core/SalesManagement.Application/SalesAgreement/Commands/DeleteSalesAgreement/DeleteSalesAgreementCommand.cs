using MediatR;

namespace SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreement
{
    public sealed record DeleteSalesAgreementCommand(int Id) : IRequest<bool>;
}
