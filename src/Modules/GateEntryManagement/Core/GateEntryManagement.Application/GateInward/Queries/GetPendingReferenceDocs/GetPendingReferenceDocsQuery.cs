using Contracts.Common;
using Contracts.Dtos.Common;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetPendingReferenceDocs
{
    /// <summary>
    /// Loads pending reference documents (e.g. Local PO, Import PO) for a given party + document type.
    /// The handler dispatches to the matching <c>IPendingReferenceDocResolver</c> based on
    /// <see cref="ReferenceDocumentTypeId"/> (= <c>Finance.TransactionTypeMaster.Id</c>).
    /// </summary>
    public class GetPendingReferenceDocsQuery
        : IRequest<ApiResponseDTO<List<PendingReferenceDocDto>>>
    {
        public int PartyId { get; set; }
        public int ReferenceDocumentTypeId { get; set; }
    }
}
