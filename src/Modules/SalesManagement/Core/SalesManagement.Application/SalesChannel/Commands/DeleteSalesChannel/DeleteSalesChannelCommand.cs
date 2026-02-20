#nullable disable
using MediatR;

namespace SalesManagement.Application.SalesChannel.Commands.DeleteSalesChannel;

public sealed record DeleteSalesChannelCommand(int Id) : IRequest<bool>;
