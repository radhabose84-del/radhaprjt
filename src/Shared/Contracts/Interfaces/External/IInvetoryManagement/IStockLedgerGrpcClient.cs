using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Stock;

namespace Contracts.Interfaces.External.IInvetoryManagement
{
    public interface IStockLedgerGrpcClient
    {
        Task<bool>
            InsertStockLedgerAsync(
                List<StockLedgerDto> stockLedgers,
                CancellationToken ct = default);

        Task<bool>
            InsertSubStoreStockLedgerAsync(
                List<SubStoreStockLedgerDto> subStoreStockLedgers,
               CancellationToken ct = default);
    }
}