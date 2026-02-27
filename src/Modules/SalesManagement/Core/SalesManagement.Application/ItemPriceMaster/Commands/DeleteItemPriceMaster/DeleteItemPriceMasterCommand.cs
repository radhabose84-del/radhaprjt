using MediatR;

namespace SalesManagement.Application.ItemPriceMaster.Commands.DeleteItemPriceMaster;

public sealed record DeleteItemPriceMasterCommand(int Id) : IRequest<bool>;
