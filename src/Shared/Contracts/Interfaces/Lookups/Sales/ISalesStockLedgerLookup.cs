// This interface has been replaced by ISalesStockLedgerService.
// ISalesStockLedgerLookup was removed because Sales.StockLedger is a transactional table
// and should never be cached by the global CachedLookupDecorator (which wraps all *Lookup interfaces).
// Use ISalesStockLedgerService instead.
namespace Contracts.Interfaces.Lookups.Sales { }
