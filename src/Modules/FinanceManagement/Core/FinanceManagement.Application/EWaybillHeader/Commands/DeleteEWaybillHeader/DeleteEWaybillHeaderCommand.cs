using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Commands.DeleteEWaybillHeader
{
    public sealed record DeleteEWaybillHeaderCommand(int Id) : IRequest<bool>;
}
