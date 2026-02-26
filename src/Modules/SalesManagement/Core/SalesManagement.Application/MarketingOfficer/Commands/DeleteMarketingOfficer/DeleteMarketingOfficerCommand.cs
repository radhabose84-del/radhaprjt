using MediatR;

namespace SalesManagement.Application.MarketingOfficer.Commands.DeleteMarketingOfficer
{
    public sealed record DeleteMarketingOfficerCommand(int Id) : IRequest<bool>;
}
