using MediatR;

namespace SalesManagement.Application.SalesItemPriceMaster.Commands.DeleteSalesItemPriceMaster;

public sealed record DeleteSalesItemPriceMasterCommand(int Id) : IRequest<bool>;
