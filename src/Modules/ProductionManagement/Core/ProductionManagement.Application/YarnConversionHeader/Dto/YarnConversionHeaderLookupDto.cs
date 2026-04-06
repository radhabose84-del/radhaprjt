namespace ProductionManagement.Application.YarnConversionHeader.Dto
{
    public class YarnConversionHeaderLookupDto
    {
        public int Id { get; set; }
        public string? ConversionDocNo { get; set; }
        public DateOnly ConversionDate { get; set; }
    }
}
