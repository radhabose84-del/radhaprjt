using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionMaster
{
    public sealed record DeleteGstrSectionMasterCommand(int Id) : IRequest<bool>;
}
