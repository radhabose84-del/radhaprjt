namespace Contracts.Dtos.Purchase
{
    public class IndentDto
    {
        public int Id { get; set; }
        public string IndentNumber { get; set; } = default!;
        public string IndentDate { get; set; } = default!;
        public int IndentTypeId { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public string Purpose { get; set; } = default!;
        public ICollection<IndentDetailDto> IndentDetails { get; set; } = default!;
    }
}