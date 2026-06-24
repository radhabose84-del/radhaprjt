namespace PurchaseManagement.Application.Common.Dtos
{
    /// <summary>
    /// Read-only preview of a document sequence for a screen: the last number actually issued
    /// (newest row in the table) and the next number that would be generated on create — a
    /// non-consuming peek (the real number is assigned/incremented only at create/convert time).
    /// </summary>
    public sealed class DocumentNumberPeekDto
    {
        public string? LastNumber { get; set; }
        public string? NextNumber { get; set; }
    }
}
