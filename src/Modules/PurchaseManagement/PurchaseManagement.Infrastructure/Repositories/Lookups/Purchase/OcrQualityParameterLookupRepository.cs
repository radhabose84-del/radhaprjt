using System.Data;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Purchase;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase
{
    /// <summary>
    /// Resolves the cotton-quality parameters captured on the OCR Entry behind an Arrival.
    /// All four tables live in the Purchase schema, so the chain is a same-module JOIN.
    /// Scoped to the exact OCR (OCRQualityParameter.OcrId = OCREntry.Id) and the OCR's template.
    /// </summary>
    internal sealed class OcrQualityParameterLookupRepository : IOcrQualityParameterLookup
    {
        private readonly IDbConnection _dbConnection;

        public OcrQualityParameterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<OcrQualityParameterLookupDto>> GetByArrivalHeaderIdAsync(
            int arrivalHeaderId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT  oqp.ParamId AS ParamId,
                        oqp.Value   AS Value
                FROM Purchase.ArrivalHeader ah
                INNER JOIN Purchase.RawMaterialPOHeader po ON po.Id = ah.RawMaterialPOId AND po.IsDeleted = 0
                INNER JOIN Purchase.OCREntry ocr           ON ocr.Id = po.OcrId AND ocr.IsDeleted = 0
                INNER JOIN Purchase.OCRQualityParameter oqp
                        ON oqp.OcrId = ocr.Id
                       AND oqp.QualityTemplateId = ocr.QualityTemplateId
                       AND oqp.IsDeleted = 0
                WHERE ah.Id = @ArrivalHeaderId AND ah.IsDeleted = 0
                ORDER BY oqp.Id ASC;";

            var rows = await _dbConnection.QueryAsync<OcrQualityParameterLookupDto>(
                new CommandDefinition(sql, new { ArrivalHeaderId = arrivalHeaderId }, cancellationToken: ct));
            return rows.ToList();
        }
    }
}
