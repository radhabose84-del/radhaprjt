namespace PurchaseManagement.Application.Common.Exceptions;

public sealed class LineOwnershipMismatchException : Exception
{
    public int RfqId { get; }
    public string EntityName { get; }
    public IReadOnlyList<int> LineIds { get; }

    public LineOwnershipMismatchException(string entityName, int rfqId, IEnumerable<int> lineIds)
        : base($"{entityName} id(s) do not belong to RFQ {rfqId}: {string.Join(", ", lineIds.Distinct())}")
    {
        EntityName = entityName;
        RfqId = rfqId;
        LineIds = lineIds.Distinct().ToArray();
    }
}
