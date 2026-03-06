namespace SalesManagement.Application.DispatchAdvice.Dto
{
    public sealed class DispatchAdviceLookupDto
    {
        public int Id { get; set; }
        public string? DispatchNo { get; set; }
        public DateOnly DispatchDate { get; set; }
    }
}
