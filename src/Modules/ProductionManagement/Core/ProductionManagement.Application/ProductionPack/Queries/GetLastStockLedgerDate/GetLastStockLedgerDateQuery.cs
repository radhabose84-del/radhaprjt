using MediatR;

namespace ProductionManagement.Application.ProductionPack.Queries.GetLastStockLedgerDate
{
    public class GetLastStockLedgerDateQuery : IRequest<DateOnly?>
    {
    }
}
