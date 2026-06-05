namespace Contracts.Dtos.Lookups.QC
{
    /// <summary>
    /// Cross-module lookup row for a single parameter belonging to a QC Quality Template
    /// (Qc.QualityTemplateParameter joined to Qc.QualityParameter). Drives the dynamic
    /// cotton-parameter UI on the OCR Entry form and validates stored parameter ids.
    /// </summary>
    public sealed class QualityTemplateParameterLookupDto
    {
        /// <summary>Qc.QualityParameter.Id — the value stored as OCRQualityParameter.ParamId.</summary>
        public int QualityParameterId { get; set; }
        public string? ParameterCode { get; set; }
        public string? ParameterName { get; set; }
        public int SequenceNo { get; set; }
        public bool IsMandatory { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
    }
}
