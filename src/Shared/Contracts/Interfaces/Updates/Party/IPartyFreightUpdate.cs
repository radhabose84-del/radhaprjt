using System.Data;

namespace Contracts.Interfaces.Updates.Party
{
    public interface IPartyFreightUpdate
    {
        /// <summary>
        /// Sets Party.PartyMaster.SalesFreightId only when it is currently NULL.
        /// Executes on the supplied connection + transaction so it participates in the caller's atomic transaction.
        /// </summary>
        Task UpdateSalesFreightIfNullAsync(int partyId, int freightId, IDbConnection connection, IDbTransaction transaction);
    }
}
