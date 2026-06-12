using MediatR;

namespace PurchaseManagement.Application.MixCodeMaster.Commands.DeleteMixCodeMaster
{
    public sealed record DeleteMixCodeMasterCommand(int Id) : IRequest<bool>;
}
