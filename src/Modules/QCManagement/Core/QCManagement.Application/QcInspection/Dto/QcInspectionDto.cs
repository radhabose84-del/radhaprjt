namespace QCManagement.Application.QcInspection.Dto
{
    public class QcInspectionDto
    {
        public int Id { get; set; }
        public string? QcInspectionNo { get; set; }
        public DateTimeOffset InspectionDate { get; set; }

        public int SourceTypeId { get; set; }
        public string? SourceTypeCode { get; set; }    // GRN / ARRIVAL
        public string? SourceTypeName { get; set; }
        public int SourceHeaderId { get; set; }
        public int SourceDetailId { get; set; }
        public string? SourceNo { get; set; }          // GrnNo or ArrivalNumber
        public DateTimeOffset? SourceDate { get; set; } // GrnDate or ArrivalDate
        public string? InvoiceNo { get; set; }          // GRN-only (null for Arrival)

        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }

        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? ItemCategoryId { get; set; }
        public string? ItemCategoryName { get; set; }

        public int QualitySpecificationId { get; set; }
        public string? QualitySpecificationCode { get; set; }
        public int QualityTemplateId { get; set; }
        public string? QualityTemplateCode { get; set; }
        public int QcTypeId { get; set; }

        public int InspectorUserId { get; set; }
        public string? InspectorName { get; set; }

        public decimal ReceivedQuantity { get; set; }
        public int ReceivedUomId { get; set; }
        public string? BatchNumber { get; set; }
        public string? LotNumber { get; set; }

        public int? QcStatusId { get; set; }
        public string? QcStatusCode { get; set; }
        public string? QcStatusName { get; set; }
        public decimal? AcceptedQuantity { get; set; }
        public decimal? RejectedQuantity { get; set; }
        public string? DispositionRemarks { get; set; }
        public DateTimeOffset? DispositionDate { get; set; }
        public string? DispositionByName { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public List<QcInspectionParameterResultDto> Parameters { get; set; } = new();

        // OCR cotton-quality parameters captured on the OCR behind the source Arrival, enriched
        // with the QualityParameter master (code/name/data type). The entered Value is free text.
        // Populated only for ARRIVAL-sourced inspections; empty otherwise.
        public List<QcOcrQualityParameterDto> OcrQualityParameters { get; set; } = new();

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }

    public class QcInspectionParameterResultDto
    {
        public int Id { get; set; }
        public int QualitySpecificationParameterId { get; set; }
        public int QualityParameterId { get; set; }
        public string? ParameterCode { get; set; }
        public string? ParameterName { get; set; }
        public int DataTypeId { get; set; }
        public int ValidationTypeId { get; set; }
        public string? ValidationTypeCode { get; set; }
        public int? UomId { get; set; }
        public string? UomCode { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? ExpectedValue { get; set; }
        public List<string> AllowedValues { get; set; } = new();
        public int SeverityId { get; set; }
        public string? SeverityCode { get; set; }
        public string? SeverityName { get; set; }
        public int FailureActionId { get; set; }
        public string? FailureActionCode { get; set; }
        public string? FailureActionName { get; set; }
        public int SortOrder { get; set; }
        public string? ActualValue { get; set; }
        public string? InspectionResult { get; set; }
        public string? Remarks { get; set; }

        // Free-text value entered for this parameter on the source OCR (matched by QualityParameterId
        // = OCRQualityParameter.ParamId). Null when there is no OCR or no matching OCR parameter.
        public string? OcrValue { get; set; }
    }

    /// <summary>
    /// A cotton-quality parameter captured on the OCR behind the source Arrival, enriched with
    /// the QC.QualityParameter master. <see cref="Value"/> is the free-text value entered on the OCR.
    /// </summary>
    public class QcOcrQualityParameterDto
    {
        public int QualityParameterId { get; set; }   // = OCRQualityParameter.ParamId
        public string? ParameterCode { get; set; }
        public string? ParameterName { get; set; }
        public int DataTypeId { get; set; }
        public string? Value { get; set; }
    }
}
