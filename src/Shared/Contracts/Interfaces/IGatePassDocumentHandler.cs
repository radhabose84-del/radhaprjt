using System.Data;
using System.Data.Common;

namespace Contracts.Interfaces
{
    /// <summary>
    /// Strategy interface for handling GatePass document updates.
    /// Each module implements this for its own document types (Invoice, Delivery Challan, PO, etc.).
    /// Implementations are resolved via IEnumerable&lt;IGatePassDocumentHandler&gt; in DI.
    /// </summary>
    public interface IGatePassDocumentHandler
    {
        /// <summary>
        /// The document type name matching Finance.TransactionTypeMaster.TypeName
        /// (e.g. "Invoice", "STO Delivery Challan")
        /// </summary>
        string DocumentType { get; }

        /// <summary>
        /// Marks the document as gate-passed (e.g. sets GEFlag = 1).
        /// Called within the GatePass creation transaction for atomicity.
        /// </summary>
        Task MarkAsGatePassedAsync(int documentId, DbConnection connection, DbTransaction transaction);
    }
}
