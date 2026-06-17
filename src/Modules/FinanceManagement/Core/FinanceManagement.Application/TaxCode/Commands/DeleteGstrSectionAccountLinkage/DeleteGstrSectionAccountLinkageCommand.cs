using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionAccountLinkage
{
    public sealed record DeleteGstrSectionAccountLinkageCommand(int Id) : IRequest<bool>;
}
