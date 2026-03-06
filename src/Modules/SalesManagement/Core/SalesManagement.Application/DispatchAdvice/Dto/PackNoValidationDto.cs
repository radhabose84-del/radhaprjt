namespace SalesManagement.Application.DispatchAdvice.Dto
{
    public class PackNoValidationDto
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public List<int> MissingPackNos { get; set; } = new();
    }
}
