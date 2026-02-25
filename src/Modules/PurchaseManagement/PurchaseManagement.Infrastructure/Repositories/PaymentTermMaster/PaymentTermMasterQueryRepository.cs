#nullable disable
using System.Data;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermAutoComplete;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.PaymentTermMaster
{
    public class PaymentTermMasterQueryRepository : IPaymentTermMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public PaymentTermMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<(List<PaymentTermMasterDto>, int)> GetAllPaymentTermMasterAsync(int pageNumber, int pageSize, string searchTerm)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var args = new
            {
                Search = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            const string countSql = @"
                SELECT COUNT(*)
                FROM [Purchase].[Purchase].[PaymentTermMaster] pt
                WHERE pt.IsDeleted = 0
                AND (@Search IS NULL OR @Search = '' OR (pt.Code LIKE @Search OR pt.[Description] LIKE @Search));";

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, args);

            const string pageSql = @"
                SELECT 
                    -- master columns (must be BEFORE splitOn)
                    p.Id, p.Code, p.[Description], p.BaselineTypeId, p.CreditDays,
                    p.AdvancePercent,
                    COALESCE(p.BalancePercent, 100.00 - ISNULL(p.AdvancePercent,0)) AS BalancePercent,
                    p.DiscountPercent, p.DiscountDays, p.GraceDays, p.ApplicableForPortal,
                    p.IsActive, p.IsDeleted,
                    p.CreatedBy, p.CreatedDate, p.CreatedByName, p.CreatedIP,
                    p.ModifiedBy, p.ModifiedDate, p.ModifiedByName, p.ModifiedIP,

                    -- child columns (AFTER splitOn)
                    i.Id AS InstallmentId, i.PaymentTermId, i.SeqNo, i.[Percent], i.DueDays
                    -- if you don't need installment audit fields, don't select them

                FROM (
                    SELECT *
                    FROM [Purchase].[Purchase].[PaymentTermMaster] pt
                    WHERE pt.IsDeleted = 0
                    AND (@Search IS NULL OR @Search = '' OR (pt.Code LIKE @Search OR pt.[Description] LIKE @Search))
                    ORDER BY pt.Id DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
                ) p
                LEFT JOIN [Purchase].[Purchase].[PaymentTermInstallment] i 
                    ON i.PaymentTermId = p.Id
                ORDER BY p.Id DESC, i.SeqNo;";

            var byId = new Dictionary<int, PaymentTermMasterDto>();

            await _dbConnection.QueryAsync<PaymentTermMasterDto, PaymentTermInstallmentDto, PaymentTermMasterDto>(
                pageSql,
                (master, child) =>
                {
                    if (!byId.TryGetValue(master.Id, out var dto))
                    {
                        dto = master;
                        dto.Installments = new List<PaymentTermInstallmentDto>();
                        byId.Add(dto.Id, dto);
                    }

                    if (child != null && child.SeqNo > 0)
                        dto.Installments!.Add(child);

                    return dto;
                },
                args,
                splitOn: "InstallmentId"
            );

            return (byId.Values.ToList(), totalCount);
        }
        public async Task<PaymentTermMasterDto> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT 
                    p.Id, p.Code, p.[Description], p.BaselineTypeId, p.CreditDays,
                    p.AdvancePercent,
                    COALESCE(p.BalancePercent, CONVERT(decimal(5,2), 100.00 - ISNULL(p.AdvancePercent,0))) AS BalancePercent,
                    p.DiscountPercent, p.DiscountDays, p.GraceDays, p.ApplicableForPortal ,p.IsActive,p.IsDeleted,p.CreatedBy,p.CreatedDate,p.CreatedByName,p.CreatedIP,
                    p.ModifiedBy,p.ModifiedDate,p.ModifiedByName,p.ModifiedIP
                FROM [Purchase].[Purchase].[PaymentTermMaster] p
                WHERE p.Id = @Id;

                SELECT  i.PaymentTermId, i.SeqNo, i.[Percent], i.DueDays ,i.IsActive,i.IsDeleted,i.CreatedBy,i.CreatedDate,i.CreatedByName,i.CreatedIP,i.ModifiedBy,i.ModifiedDate,i.ModifiedByName,i.ModifiedIP
                FROM [Purchase].[Purchase].[PaymentTermInstallment] i
                WHERE i.PaymentTermId = @Id
                ORDER BY i.SeqNo;";

            using var grid = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });

            var master = await grid.ReadFirstOrDefaultAsync<PaymentTermMasterDto>();
            if (master == null) return null;

            var installments = (await grid.ReadAsync<PaymentTermInstallmentDto>()).ToList();
            master.Installments = installments;

            return master;
        }

        public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
        {
            const string sql = @"
                SELECT 1
                FROM [Purchase].[Purchase].[PaymentTermMaster]
                WHERE IsDeleted = 0
                AND Code = @Code
                AND (@ExcludeId IS NULL OR Id <> @ExcludeId);";

            var exists = await _dbConnection.ExecuteScalarAsync<int?>(
                sql,
                new { Code = code, ExcludeId = excludeId }
            );

            return exists.HasValue;
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            const string sql = @"
                SELECT 1
                FROM [Purchase].[Purchase].[PaymentTermMaster]
                WHERE Id = @Id AND IsDeleted = 0;";
            var exists = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Id = id });
            return exists.HasValue;
        }
        public async Task<List<AutoCompleteDto>> GetPaymentTermAutoComplete(string searchPattern, string paymentTermCode)
        {
            const string sql = @"
            SELECT TOP (10)
                pt.Id,
                pt.Code,
                pt.[Description]
            FROM [Purchase].[Purchase].[PaymentTermMaster] pt
            WHERE pt.IsDeleted = 0
            AND pt.IsActive = 1
            AND (@PaymentTermCode IS NULL OR @PaymentTermCode = '' OR pt.Code = @PaymentTermCode)
            AND (
                    @Search IS NULL OR @Search = ''
                    OR pt.Code LIKE @Search
                    OR pt.[Description] LIKE @Search
                )
            ORDER BY
                CASE WHEN @PaymentTermCode IS NOT NULL AND @PaymentTermCode <> '' AND pt.Code = @PaymentTermCode THEN 0 ELSE 1 END,
                pt.Code;";

            var rows = await _dbConnection.QueryAsync<AutoCompleteDto>(
                sql,
                new
                {
                    PaymentTermCode = paymentTermCode,
                    Search = string.IsNullOrWhiteSpace(searchPattern) ? null : $"%{searchPattern.Trim()}%"
                });

            return rows.AsList();
        }
    


        //   public async Task<bool> ExistsByCodeAsync(string code)
    // {
    //     const string sql = @"
    //         SELECT 1
    //         FROM [Purchase].[Purchase].[PaymentTermMaster]
    //         WHERE IsDeleted = 0 AND Code = @Code";

    //     var exists = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { Code = code });
    //     return exists.HasValue;
    // }

    //    public async Task<PaymentTermMasterDto> GetByIdAsync(int id)
    //     {
    //         const string sql = @" 
    //         SELECT 
    //             p.Id, p.Code, p.[Description], p.BaselineTypeId, p.CreditDays,
    //             p.AdvancePercent,
    //             COALESCE(p.BalancePercent, 100.00 - ISNULL(p.AdvancePercent,0)) AS BalancePercent,
    //             p.DiscountPercent, p.DiscountDays, p.GraceDays, p.ApplicableForPortal,
    //             i.Id AS InstallmentId,i.PaymentTermId, i.SeqNo, i.[Percent], i.DueDays
    //         FROM [Purchase].[Purchase].[PaymentTermMaster] p
    //         LEFT JOIN [Purchase].[Purchase].[PaymentTermInstallment] i
    //             ON i.PaymentTermId = p.Id
    //         WHERE p.Id = @Id
    //         ORDER BY i.SeqNo;";

    //         PaymentTermMasterDto? master = null;

    //         await _dbConnection.QueryAsync<PaymentTermMasterDto, PaymentTermInstallmentDto, PaymentTermMasterDto>(
    //             sql,
    //             (m, child) =>
    //             {
    //                 if (master == null)
    //                 {
    //                     master = m;
    //                     master.Installments = new List<PaymentTermInstallmentDto>();
    //                 }

    //                 if (child != null && child.SeqNo > 0)
    //                     master.Installments!.Add(child);

    //                 return m;
    //             },
    //             new { Id = id },
    //             splitOn: "InstallmentId"
    //         );

    //         if (master == null)
    //             throw new KeyNotFoundException($"Payment term not found for Id={id}.");

    //         return master;
    //     }

    //     public async Task<PaymentTermMasterDto> GetByIdAsync(int id)
    // {
    //     const string sql = @"
    //     SELECT 
    //         p.Id, p.Code, p.[Description], p.BaselineTypeId, p.CreditDays,
    //         p.AdvancePercent,
    //         COALESCE(p.BalancePercent, 100.00 - ISNULL(p.AdvancePercent,0)) AS BalancePercent,
    //         p.DiscountPercent, p.DiscountDays, p.GraceDays, p.ApplicableForPortal,
    //         i.Id AS InstallmentId, i.SeqNo, i.[Percent], i.DueDays
    //     FROM [Purchase].[Purchase].[PaymentTermMaster] p
    //     LEFT JOIN [Purchase].[Purchase].[PaymentTermInstallment] i
    //         ON i.PaymentTermId = p.Id
    //     WHERE p.Id = @Id
    //     ORDER BY i.SeqNo;";

    //     PaymentTermMasterDto? master = null;

    //     await _dbConnection.QueryAsync<
    //         PaymentTermMasterDto, PaymentTermInstallmentDto, PaymentTermMasterDto>(
    //         sql,
    //         (m, child) =>
    //         {
    //             if (master == null)
    //             {
    //                 master = m;
    //                 master.Installments = new List<PaymentTermInstallmentDto>();
    //             }

    //             if (child != null && child.SeqNo > 0)
    //                 master.Installments!.Add(child);

    //             return m;
    //         },
    //         new { Id = id },
    //         splitOn: "InstallmentId",
    //         buffered: false
    //     );

    //     if (master == null)
    //         throw new KeyNotFoundException($"Payment term not found for Id={id}.");

    //     return master;
    // }

}
}