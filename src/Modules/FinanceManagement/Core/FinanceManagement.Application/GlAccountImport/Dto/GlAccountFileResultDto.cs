namespace FinanceManagement.Application.GlAccountImport.Dto
{
    /// <summary>A generated file (template or full export) ready to stream back to the client.</summary>
    public sealed class GlAccountFileResultDto
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
    }
}
