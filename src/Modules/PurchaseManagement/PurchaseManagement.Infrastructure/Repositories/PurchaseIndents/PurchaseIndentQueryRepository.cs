using System.Data;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndent;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseIndents
{
    public class PurchaseIndentQueryRepository : IPurchaseIndentQuery, IPurchaseIndentGrpcQuery
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        private readonly IUnitLookup _unitLookup;

<<<<<<< HEAD
        public PurchaseIndentQueryRepository(IDbConnection dbConnection, IIPAddressService iPAddressService, IUnitLookup unitLookup)
=======
        public PurchaseIndentQueryRepository(IDbConnection dbConnection, IIPAddressService iPAddressService,
            IUnitLookup unitLookup)
>>>>>>> remotes/origin/BSOFTERP_Dev_V0.1
        {
            _dbConnection = dbConnection;
            _ipAddressService = iPAddressService;
            _unitLookup = unitLookup;
        }

        public async Task<string> GeneratePurchaseIndentNumberAsync(int unitId)
        {
<<<<<<< HEAD
            var unit = await _unitLookup.GetByIdAsync(unitId);
            if (unit == null)
                throw new ExceptionRules("Invalid Unit Id. Failed to generate indent number.");

            const string sql = @"
                SELECT MAX(CAST(RIGHT(IndentNumber, 4) AS INT))
                FROM Purchase.IndentHeader
                WHERE UnitId = @UnitId";

            int? maxSequence = await _dbConnection.ExecuteScalarAsync<int?>(sql, new { UnitId = unitId });
            int newSequence = (maxSequence ?? 0) + 1;
            return $"PI/{unit.ShortName}/{newSequence:D4}";
=======
            var units = await _unitLookup.GetAllUnitAsync();
            var unitDict = units.ToDictionary(d => d.UnitId, d => d.ShortName);

            if (!unitDict.TryGetValue(unitId, out var unitCode))
            {
                throw new Exception("Invalid Unit Id. Failed to generate indent number.");
            }


            const string sql = @"
                   SELECT MAX(CAST(RIGHT(IndentNumber, 4) AS INT))
                   FROM Purchase.IndentHeader
                   WHERE UnitId = @UnitId 
                         ";

            int? maxSequence = await _dbConnection.ExecuteScalarAsync<int?>(sql, new
            {
                UnitId = unitId
            });

            int newSequence = (maxSequence ?? 0) + 1;


            string indentNumber = $"PI/{unitCode}/{newSequence:D4}";

            return indentNumber;
>>>>>>> remotes/origin/BSOFTERP_Dev_V0.1
        }

        // // public async Task<(List<IndentHeader>, int)> GetAllPurchaseIndentAsync(int PageNumber, int PageSize, string? SearchTerm, int? StatusId)
        // // {
        // //     var UnitId = _ipAddressService.GetUnitId() ?? 0;
        // //     const string dataQuery = @" SELECT 
        // //         IH.Id, 
        // //         IH.IndentNumber,
        // //         IH.IndentDate,
        // //         IH.IndentTypeId,
        // //         IH.UnitId,
        // //         IH.DepartmentId,
        // //         IH.Purpose,
        // //         IH.IsActive,IH.CreatedDate,IH.CreatedBy,IH.CreatedByName,IH.ModifiedBy,IH.ModifiedDate,IH.ModifiedByName,IndentType.Id,IndentType.Code,
        // //         Status.Id,Status.Code,
        // //         (
        // //             SELECT COUNT(1)
        // //             FROM Purchase.IndentDetail IDT
        // //             WHERE IDT.IndentHeaderId = IH.Id
        // //             AND IDT.IsDeleted = 0
        // //         ) AS ItemCount

        // //     FROM [Purchase].[IndentHeader] IH
        // //     INNER JOIN [Purchase].[MiscMaster] IndentType on IH.IndentTypeId=IndentType.Id
        // //     INNER JOIN [Purchase].[MiscMaster] Status on IH.StatusId=Status.Id
        // //     WHERE 
        // //     IH.IsDeleted = 0
        // //         AND (@Search IS NULL OR IndentType.Code LIKE @Search OR IH.IndentNumber LIKE @Search)
        // //         AND  IH.UnitId=@UnitId AND ( @StatusId IS NULL OR IH.StatusId=@StatusId)
        // //         ORDER BY IH.Id desc
        // //         OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        // //     ";
        // //     const string countQuery = @"
        // //       SELECT COUNT(*) 
        // //        FROM [Purchase].[IndentHeader] IH
        // //     INNER JOIN [Purchase].[MiscMaster] IndentType on IH.IndentTypeId=IndentType.Id
        // //     WHERE 
        // //     IH.IsDeleted = 0
        // //         AND (@Search IS NULL OR IndentType.Code LIKE @Search OR IH.IndentNumber LIKE @Search)
        // //         AND ( @StatusId IS NULL OR IH.StatusId=@StatusId)
        // //         AND  IH.UnitId=@UnitId;
        // //   ";


        // //     var parameters = new
        // //     {
        // //         Search = string.IsNullOrEmpty(SearchTerm) ? null : $"%{SearchTerm}%",
        // //         Offset = (PageNumber - 1) * PageSize,
        // //         PageSize,
        // //         UnitId,
        // //         StatusId
        // //     };

        // //     var Indent = await _dbConnection.QueryAsync<IndentHeader, PurchaseManagement.Domain.Entities.MiscMaster, PurchaseManagement.Domain.Entities.MiscMaster, IndentHeader>(
        // //         dataQuery,
        // //         (indentHeader, indentType, status) =>
        // //         {
        // //             indentHeader.IndentType = new PurchaseManagement.Domain.Entities.MiscMaster
        // //             {
        // //                 Id = indentType.Id,
        // //                 Code = indentType.Code
        // //             };
        // //             indentHeader.Status = new PurchaseManagement.Domain.Entities.MiscMaster
        // //             {
        // //                 Id = status.Id,
        // //                 Code = status.Code
        // //             };

        // //             return indentHeader;
        // //         },
        // //         parameters,
        // //         splitOn: "Id,Id"
        // //         );

        // //     var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery, parameters);

        // //     return (Indent.ToList(), totalCount);
        // // }

        public async Task<IndentHeader?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT 
                IH.Id, 
                IH.IndentNumber,
                IH.IndentDate,
                IH.IndentTypeId,
                IH.UnitId,
                IH.DepartmentId,
                IH.Purpose,
                IH.IsActive,
                ID.Id,ID.IndentHeaderId,ID.ItemId,ID.ItemCategoryId,ID.ItemUOMId,ID.Rate,
                ID.QuantityRequired,ID.RequiredDate,ID.TotalEstimatedCost,ID.PRConsumptionDays,ID.Remark,ID.IsActive,ID.IsRFQDone,HeaderStatus.Id,HeaderStatus.Code,
                LineStatus.Id,LineStatus.Code,IndentType.Id,IndentType.Code
            FROM [Purchase].[IndentHeader] IH
            INNER JOIN [Purchase].[IndentDetail] ID on ID.IndentHeaderId=IH.Id
            INNER JOIN [Purchase].[MiscMaster] HeaderStatus on IH.StatusId=HeaderStatus.Id
            INNER JOIN [Purchase].[MiscMaster] LineStatus on ID.StatusId=LineStatus.Id
            INNER JOIN [Purchase].[MiscMaster] IndentType on IH.IndentTypeId=IndentType.Id
            WHERE IH.IsDeleted = 0 
              AND IH.IsActive = 1 AND IH.Id = @Id;";

            var IndentDictionary = new Dictionary<int, IndentHeader>();

            var IndentResponse = await _dbConnection.QueryAsync<IndentHeader, IndentDetail, PurchaseManagement.Domain.Entities.MiscMaster, PurchaseManagement.Domain.Entities.MiscMaster, PurchaseManagement.Domain.Entities.MiscMaster, IndentHeader>(
                query,
                (indentHeader, indentDetail, headerStatus, lineStatus, indentType) =>
                {
                    if (!IndentDictionary.TryGetValue(indentHeader.Id, out var existingIndentHeader))
                    {
                        existingIndentHeader = indentHeader;
                        existingIndentHeader.IndentDetails = new List<IndentDetail>();
                        indentHeader.Status = new PurchaseManagement.Domain.Entities.MiscMaster
                        {
                            Id = headerStatus.Id,
                            Code = headerStatus.Code
                        };
                        indentHeader.IndentType = new PurchaseManagement.Domain.Entities.MiscMaster
                        {
                            Id = indentType.Id,
                            Code = indentType.Code
                        };

                        IndentDictionary[indentHeader.Id] = existingIndentHeader;
                    }
                    if (indentDetail is not null)
                    {
                        indentDetail.Status = lineStatus is null
                            ? null
                            : new PurchaseManagement.Domain.Entities.MiscMaster
                            {
                                Id = lineStatus.Id,
                                Code = lineStatus.Code
                            };

                        if (!existingIndentHeader.IndentDetails!
                              .Any(a => a.Id == indentDetail.Id))
                        {

                            existingIndentHeader.IndentDetails.Add(indentDetail);
                        }
                    }



                    return existingIndentHeader;
                },
                new { id },
                splitOn: "Id,Id,Id,Id"
                );

            return IndentResponse.FirstOrDefault()!;
        }

        public async Task<IndentHeader> GetByIdGrpcAsync(int id)
        {
            const string query = @"
                SELECT 
                IH.Id, 
                IH.IndentNumber,
                IH.IndentDate,
                IH.IndentTypeId,
                IH.UnitId,
                IH.DepartmentId,
                IH.Purpose,
                IH.CreatedBy,IH.CreatedDate,
                ID.Id,ID.IndentHeaderId,ID.ItemId,
                ID.QuantityRequired,ID.RequiredDate,ID.TotalEstimatedCost,ID.PRConsumptionDays,ID.Remark,ID.IsActive
            FROM [Purchase].[IndentHeader] IH
            INNER JOIN [Purchase].[IndentDetail] ID on ID.IndentHeaderId=IH.Id
            WHERE IH.IsDeleted = 0 
              AND IH.IsActive = 1 AND IH.Id = @Id;";

            var IndentDictionary = new Dictionary<int, IndentHeader>();

            var IndentResponse = await _dbConnection.QueryAsync<IndentHeader, IndentDetail, IndentHeader>(
                query,
                (indentHeader, indentDetail) =>
                {
                    if (!IndentDictionary.TryGetValue(indentHeader.Id, out var existingIndentHeader))
                    {
                        existingIndentHeader = indentHeader;
                        existingIndentHeader.IndentDetails = new List<IndentDetail>();
                        IndentDictionary[indentHeader.Id] = existingIndentHeader;
                    }
                    if (!existingIndentHeader.IndentDetails!
                       .Any(a => a.Id == indentDetail.Id))
                    {
                        existingIndentHeader.IndentDetails.Add(indentDetail);
                    }


                    return existingIndentHeader;
                },
                new { id },
                splitOn: "Id"
                );

            return IndentResponse.FirstOrDefault()!;
        }



        public async Task<bool> NotFoundAsync(int id)
        {
            var query = "SELECT COUNT(1) FROM [Purchase].[IndentHeader]  WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { Id = id });
            return count > 0;
        }

        public async Task<(List<PendingIndentDto>, int)> GetPendingPurchaseIndentAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var sql = @"
                    SELECT 
                        IH.Id,
                        IH.IndentNumber,
                        IH.IndentDate,
                        IH.IndentTypeId,
                        IndentType.Code AS IndentType,
                        IH.UnitId,
                        IH.DepartmentId,
                        IH.Purpose,
                        IH.IsActive,
                        IH.CreatedDate,
                        IH.CreatedBy,
                        IH.CreatedByName,
                        COUNT(IDT.Id) AS ItemCount,
                        COUNT(*) OVER() AS TotalCount
                    FROM Purchase.IndentHeader IH
                    INNER JOIN Purchase.IndentDetail IDT 
                        ON IDT.IndentHeaderId = IH.Id
                        AND IDT.IsDeleted = 0
                    INNER JOIN Purchase.MiscMaster IndentType 
                        ON IH.IndentTypeId = IndentType.Id
                    INNER JOIN Purchase.MiscMaster IndentStatus 
                        ON IH.StatusId = IndentStatus.Id
                    WHERE IH.IsDeleted = 0
                    AND IH.UnitId = @UnitId
                    AND IndentStatus.Code = @Pending
                    AND (
                            @Search IS NULL
                            OR IH.IndentNumber LIKE @Search
                            OR IndentType.Code LIKE @Search
                        )
                    GROUP BY 
                        IH.Id,
                        IH.IndentNumber,
                        IH.IndentDate,
                        IH.IndentTypeId,
                        IndentType.Code,
                        IH.UnitId,
                        IH.DepartmentId,
                        IH.Purpose,
                        IH.IsActive,
                        IH.CreatedDate,
                        IH.CreatedBy,
                        IH.CreatedByName
                    ORDER BY IH.Id DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                    ";

            var parameters = new
            {
                UnitId = unitId,
                Pending = MiscEnumEntity.Pending,
                Search = string.IsNullOrEmpty(SearchTerm) ? null : $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize = PageSize
            };

            var result = (await _dbConnection.QueryAsync<PendingIndentDto, int, PendingIndentDto>(
                sql,
                (indent, totalCount) =>
                {
                    // TotalCount will be read separately
                    return indent;
                },
                parameters,
                splitOn: "TotalCount"
            )).ToList();

            var totalCount = result.FirstOrDefault()?.ItemCount > 0
                ? result.FirstOrDefault()!.ItemCount
                : 0;

            return (result, totalCount);
        }

        // public async Task<(List<IndentHeader>, int)> GetPendingPurchaseIndentAsync(int PageNumber, int PageSize, string? SearchTerm)
        // {
        //     var UnitId = _ipAddressService.GetUnitId() ?? 0;
        //     const string dataQuery = @" SELECT 
        //         IH.Id, 
        //         IH.IndentNumber,
        //         IH.IndentDate,
        //         IH.IndentTypeId,
        //         IH.UnitId,
        //         IH.DepartmentId,
        //         IH.Purpose,
        //         IH.IsActive,IH.CreatedDate,IH.CreatedBy,IH.CreatedByName,IH.ModifiedBy,IH.ModifiedDate,IH.ModifiedByName,IndentType.Id,IndentType.Code,
        //         COUNT(IDT.Id) AS ItemCount 
        //     FROM [Purchase].[IndentHeader] IH
        //     INNER JOIN [Purchase].[IndentDetail] IDT on IDT.IndentHeaderId=IH.Id
        //     INNER JOIN [Purchase].[MiscMaster] IndentType on IH.IndentTypeId=IndentType.Id
        //     INNER JOIN [Purchase].[MiscMaster] IndentStatus on IH.StatusId=IndentStatus.Id
        //     WHERE 
        //     IH.IsDeleted = 0
        //         AND (@Search IS NULL OR IndentType.Code LIKE @Search OR IH.IndentNumber LIKE @Search)
        //         AND  IH.UnitId=@UnitId AND IndentStatus.Code = @Pending
        //         GROUP BY 
        //         IH.Id, 
        //         IH.IndentNumber,
        //         IH.IndentDate,
        //         IH.IndentTypeId,
        //         IH.UnitId,
        //         IH.DepartmentId,
        //         IH.Purpose,
        //         IH.IsActive,IH.CreatedDate,IH.CreatedBy,IH.CreatedByName,IH.ModifiedBy,IH.ModifiedDate,IH.ModifiedByName,IndentType.Id,IndentType.Code
        //         ORDER BY IH.Id desc
        //         OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        //     ";
        //     const string countQuery = @"
        //       SELECT COUNT(*) 
        //        FROM [Purchase].[IndentHeader] IH
        //     INNER JOIN [Purchase].[MiscMaster] IndentType on IH.IndentTypeId=IndentType.Id
        //     INNER JOIN [Purchase].[MiscMaster] IndentStatus on IH.StatusId=IndentStatus.Id
        //     WHERE 
        //     IH.IsDeleted = 0
        //         AND (@Search IS NULL OR IndentType.Code LIKE @Search OR IH.IndentNumber LIKE @Search)
        //         AND  IH.UnitId=@UnitId AND IndentStatus.Code = @Pending;
        //   ";


        //     var parameters = new
        //     {
        //         Search = string.IsNullOrEmpty(SearchTerm) ? null : $"%{SearchTerm}%",
        //         Offset = (PageNumber - 1) * PageSize,
        //         PageSize,
        //         UnitId,
        //         Pending = MiscEnumEntity.Pending
        //     };

        //     var Indent = await _dbConnection.QueryAsync<IndentHeader, PurchaseManagement.Domain.Entities.MiscMaster, IndentHeader>(
        //         dataQuery,
        //         (indentHeader, indentType) =>
        //         {
        //             indentHeader.IndentType = new PurchaseManagement.Domain.Entities.MiscMaster
        //             {
        //                 Id = indentType.Id,
        //                 Code = indentType.Code
        //             };

        //             return indentHeader;
        //         },
        //         parameters,
        //         splitOn: "Id"
        //         );

        //     var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery, parameters);

        //     return (Indent.ToList(), totalCount);
        // }

        public async Task<List<IndentHeader>> GetPurchaseIndentAutoCompleteAsync(string status, string searchTerm, bool allIndents = false)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var searchPattern = string.IsNullOrWhiteSpace(searchTerm)
                ? "%"
                : $"%{searchTerm.Trim()}%";

            var sql = @"
                SELECT 
                    IH.Id,
                    IH.IndentNumber
                FROM [Purchase].[IndentHeader] IH
                INNER JOIN [Purchase].[MiscMaster] ST ON IH.StatusId = ST.Id
                WHERE
                    ST.Code = @Status
                    AND IH.IsDeleted = 0
                    AND IH.IsActive = 1
                    AND IH.UnitId = @UnitId
                    AND IH.IndentNumber LIKE @Search
                ";

            // When AllIndents = false => hide indents that already moved forward (RFQ/QC/PO exists)
            if (!allIndents)
            {
                sql += @"
                    AND NOT EXISTS (
                        SELECT 1
                        FROM [Purchase].[RfqMaster] RH
                        WHERE RH.IndentId = IH.Id
                        AND RH.IsDeleted = 0
                    )   
                    AND NOT EXISTS (
                        SELECT 1
                        FROM [Purchase].[PurchaseLocalDetail] POH
                        WHERE POH.IndentId = IH.Id
                        AND POH.IsDeleted = 0
                    )
                ";
            }

            var cmd = new Dapper.CommandDefinition(
                sql,
                new { Status = status, UnitId = unitId, Search = searchPattern }
            );

            var indent = await _dbConnection.QueryAsync<IndentHeader>(cmd);
            return indent.ToList();
        }

        public async Task<List<IndentForPODto>> GetApprovedIndentDetailsForPO(int? vendorId, int? departmentId, CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string query = @"
                SELECT
                    -- ===== Header DTO =====
                    IH.Id                                  AS Id,
                    IH.IndentNumber                         AS IndentNumber,
                    CAST(IH.IndentDate AS date)             AS IndentDate,
                    IH.IndentTypeId                         AS IndentTypeId,
                    IndentType.Code                         AS IndentTypeName,
                    IH.UnitId                               AS UnitId,
                    IH.DepartmentId                         AS DepartmentId,
                    IH.Purpose                              AS Purpose,

                    RFQINFO.RfqId                           AS RfqId,
                    QUOINFO.QuotationId                     AS QuotationId,
                    CASE WHEN (PICK.UnitPrice IS NOT NULL AND PICK.UnitPrice > 0) THEN 1 ELSE 0 END AS HasPriceMaster,
                    NULLIF(
                        LTRIM(RTRIM(
                            STUFF(
                                CONCAT(
                                    CASE 
                                        WHEN RFQINFO.RfqId IS NOT NULL AND QUOINFO.QuotationId IS NULL 
                                        THEN '; RFQ created but quotation entry not yet created' 
                                        ELSE '' 
                                    END,
                                    CASE 
                                        WHEN (@VendorId IS NOT NULL AND @VendorId > 0)
                                            AND (PICK.UnitPrice IS NULL OR PICK.UnitPrice <= 0)
                                        THEN '; Price master not yet updated for this indent for selected vendor'
                                        ELSE ''
                                    END
                                ), 1, 2, ''
                            )
                        )),
                    '') AS PendingReason,

                    -- ===== Detail DTO starts here (splitOn = IndentHeaderId) =====
                    ID.IndentHeaderId                       AS IndentHeaderId,
                    ID.Id                                   AS Id,
                    ID.ItemId                               AS ItemId,
                    ID.ItemCategoryId                       AS ItemCategoryId,
                    ID.ItemUOMId                            AS ItemUOMId,
                    COALESCE(PICK.UnitPrice, ID.Rate)        AS UnitPrice,
                    (ID.QuantityRequired - ISNULL(POQ.ApprovedPOQty,0)) AS QuantityRequired,
                    CAST(ID.RequiredDate AS date)            AS RequiredDate,
                    ID.TotalEstimatedCost                   AS TotalEstimatedCost,
                    ID.PRConsumptionDays                    AS PRConsumptionDays,
                    ID.Remark                               AS Remark,
                    ID.IsRFQDone                            AS IsRFQDone

                FROM Purchase.IndentHeader IH
                INNER JOIN Purchase.IndentDetail ID ON ID.IndentHeaderId = IH.Id
                INNER JOIN Purchase.MiscMaster HeaderStatus ON IH.StatusId = HeaderStatus.Id
                INNER JOIN Purchase.MiscMaster LineStatus   ON ID.StatusId = LineStatus.Id
                INNER JOIN Purchase.MiscMaster IndentType   ON IH.IndentTypeId = IndentType.Id

                OUTER APPLY (
                    SELECT SUM(POD.Quantity) AS ApprovedPOQty
                    FROM Purchase.PurchaseLocalDetail POD
                    WHERE POD.IndentId = ID.Id
                ) POQ

                OUTER APPLY (
                    SELECT TOP (1) RM.Id AS RfqId
                    FROM Purchase.RfqMaster RM
                    WHERE RM.IndentId = IH.Id 
                    ORDER BY RM.Id DESC
                ) RFQINFO

                OUTER APPLY (
                    SELECT TOP (1) QH.Id AS QuotationId
                    FROM Purchase.QuotationHeader QH
                    WHERE QH.RfqId = RFQINFO.RfqId
                    ORDER BY QH.Id DESC
                ) QUOINFO

                OUTER APPLY (
                    SELECT TOP (1) PD.UnitPrice
                    FROM Purchase.PriceMasterHeader PH
                    INNER JOIN Purchase.MiscMaster PMStatus ON PMStatus.Id = PH.StatusId
                    INNER JOIN Purchase.PriceMasterDetail PD ON PD.PriceMasterHeaderId = PH.Id
                    WHERE PH.UnitId = IH.UnitId
                    AND PH.ItemId = ID.ItemId
                    AND (@VendorId IS NULL OR PH.VendorId = @VendorId)
                    AND PMStatus.Code = @PriceApprovedStatus
                    AND CAST(getdate() AS date) BETWEEN CAST(PH.ValidFrom AS date)
                        AND CAST(ISNULL(PH.ValidTo,'9999-12-31') AS date)
                    AND (PD.ScaleQtyFrom IS NULL OR PD.ScaleQtyFrom <= (ID.QuantityRequired - ISNULL(POQ.ApprovedPOQty,0)))
                    AND (PD.ScaleQtyTo   IS NULL OR (ID.QuantityRequired - ISNULL(POQ.ApprovedPOQty,0)) <= PD.ScaleQtyTo)
                    ORDER BY PH.ValidFrom DESC, ISNULL(PD.ScaleQtyFrom,0) DESC, PD.Id DESC
                ) PICK

                WHERE IH.IsDeleted = 0
                AND IH.IsActive  = 1
                AND IH.UnitId    = @UnitId
                AND HeaderStatus.Code = @IndentApprovedStatus
                AND LineStatus.Code   = @IndentApprovedStatus
                AND (ID.QuantityRequired - ISNULL(POQ.ApprovedPOQty,0)) > 0
                AND (@DepartmentId IS NULL OR IH.DepartmentId = @DepartmentId)
                AND NOT EXISTS (
                    SELECT 1
                    FROM Purchase.PurchaseLocalDetail POD2
                    INNER JOIN Purchase.IndentDetail ID2 ON ID2.Id = POD2.IndentId
                    WHERE ID2.IndentHeaderId = IH.Id
                )order by UnitPrice desc ;";

            var param = new
            {
                UnitId = unitId,
                VendorId = vendorId,
                DepartmentId = departmentId,
                IndentApprovedStatus = MiscEnumEntity.Approved.ToString(),
                PriceApprovedStatus = MiscEnumEntity.Approved.ToString()
            };

            var dict = new Dictionary<int, IndentForPODto>();

            await _dbConnection.QueryAsync<IndentForPODto, IndentDetailsForPODto, IndentForPODto>(
                query,
                (h, d) =>
                {
                    if (!dict.TryGetValue(h.Id, out var existing))
                    {
                        existing = h;
                        existing.IndentDetails ??= new List<IndentDetailsForPODto>();
                        existing.IndentDutyDetails ??= new List<IndentDutyForPODto>();
                        dict[h.Id] = existing;
                    }
                    else
                    {
                        // merge header-level values across rows (important!)
                        existing.RfqId ??= h.RfqId;
                        existing.QuotationId ??= h.QuotationId;

                        if (existing.HasPriceMaster is null) existing.HasPriceMaster = h.HasPriceMaster;
                        else if ((existing.HasPriceMaster ?? 0) == 1 && (h.HasPriceMaster ?? 0) == 0) existing.HasPriceMaster = 0;

                        existing.PendingReason = MergeReason(existing.PendingReason, h.PendingReason);
                    }

                    if (d != null && !existing.IndentDetails.Any(x => x.Id == d.Id))
                        existing.IndentDetails.Add(d);

                    return existing;
                },
                param,
                splitOn: "IndentHeaderId"
            );

            return dict.Values.ToList();
        }

        private static string MergeReason(string a, string b)
        {
            a = string.IsNullOrWhiteSpace(a) ? null : a.Trim();
            b = string.IsNullOrWhiteSpace(b) ? null : b.Trim();
            if (a is null) return b;
            if (b is null) return a;

            // avoid duplicates
            if (a.Contains(b, StringComparison.OrdinalIgnoreCase)) return a;
            return $"{a}; {b}";
        }

        public async Task<(List<PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto> Items, int TotalCount)> GetAllPurchaseIndentAsync(int pageNumber, int pageSize, string searchTerm, int? statusId)
        {
           var unitId = _ipAddressService.GetUnitId() ?? 0;

                    var sql = @"
                        SELECT 
                            IH.Id,
                            IH.IndentNumber,
                            IH.IndentDate,
                            IH.IndentTypeId,
                            IndentType.Code AS IndentType,
                            IH.UnitId,
                            IH.DepartmentId,
                            IH.Purpose,
                            Status.Code AS Status,
                            IH.IsActive,
                            IH.CreatedDate,
                            IH.CreatedByName,

                            COUNT(IDT.Id) AS ItemCount,

                            CASE 
                                WHEN SUM(ISNULL(IDT.POQty, 0)) = 0 THEN 'Open'
                                WHEN SUM(ISNULL(IDT.POQty, 0)) < SUM(ISNULL(IDT.QuantityRequired, 0)) THEN 'Partial'
                                ELSE 'Closed'
                            END AS POStatus,

                            COUNT(*) OVER() AS TotalCount
                        FROM Purchase.IndentHeader IH
                        INNER JOIN Purchase.MiscMaster IndentType 
                            ON IH.IndentTypeId = IndentType.Id
                        INNER JOIN Purchase.MiscMaster Status 
                            ON IH.StatusId = Status.Id
                        LEFT JOIN Purchase.IndentDetail IDT 
                            ON IDT.IndentHeaderId = IH.Id
                            AND IDT.IsDeleted = 0
                        WHERE IH.IsDeleted = 0
                        AND IH.UnitId = @UnitId
                        AND (@StatusId IS NULL OR IH.StatusId = @StatusId)
                        AND (
                                @Search IS NULL
                                OR IH.IndentNumber LIKE @Search
                                OR IndentType.Code LIKE @Search
                            )
                        GROUP BY 
                            IH.Id,
                            IH.IndentNumber,
                            IH.IndentDate,
                            IH.IndentTypeId,
                            IndentType.Code,
                            IH.UnitId,
                            IH.DepartmentId,
                            IH.Purpose,
                            Status.Code,
                            IH.IsActive,
                            IH.CreatedDate,
                            IH.CreatedByName
                        ORDER BY IH.Id DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                    ";

                    var parameters = new
                    {
                        UnitId = unitId,
                        StatusId = statusId,
                        Search = string.IsNullOrEmpty(searchTerm) ? null : $"%{searchTerm}%",
                        Offset = (pageNumber - 1) * pageSize,
                        PageSize = pageSize
                    };

                    int totalCount = 0;

                    var data = (await _dbConnection.QueryAsync<
                        PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto,
                        int,
                        PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto>(
                        sql,
                        (indent, total) =>
                        {
                            if (totalCount == 0)
                                totalCount = total;

                            return indent;
                        },
                        parameters,
                        splitOn: "TotalCount"
                    )).ToList();

                    return (data, totalCount);
        }

        //     public async Task<(List<PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto>, int)> GetAllPurchaseIndentAsync(
        //     int pageNumber,
        //     int pageSize,
        //     string? searchTerm,
        //     int? statusId)
        // {
        //     var unitId = _ipAddressService.GetUnitId() ?? 0;

        //     var sql = @"
        //     SELECT 
        //                 IH.Id,
        //                 IH.IndentNumber,
        //                 IH.IndentDate,
        //                 IH.IndentTypeId,
        //                 IndentType.Code AS IndentType,
        //                 IH.UnitId,
        //                 IH.DepartmentId,
        //                 IH.Purpose,
        //                 Status.Code AS Status,
        //                 IH.IsActive,
        //                 IH.CreatedDate,
        //                 IH.CreatedByName,

        //                 COUNT(IDT.Id) AS ItemCount,

        //                 CASE 
        //                     WHEN SUM(ISNULL(IDT.POQty, 0)) = 0 
        //                         THEN 'Open'
        //                     WHEN SUM(ISNULL(IDT.POQty, 0)) < SUM(ISNULL(IDT.QuantityRequired, 0))
        //                         THEN 'Partial'
        //                     ELSE 'Closed'
        //                 END AS POStatus,

        //                 COUNT(*) OVER() AS TotalCount

        //             FROM Purchase.IndentHeader IH
        //             INNER JOIN Purchase.MiscMaster IndentType 
        //                 ON IH.IndentTypeId = IndentType.Id
        //             INNER JOIN Purchase.MiscMaster Status 
        //                 ON IH.StatusId = Status.Id
        //             LEFT JOIN Purchase.IndentDetail IDT 
        //                 ON IDT.IndentHeaderId = IH.Id
        //             AND IDT.IsDeleted = 0

        //             WHERE IH.IsDeleted = 0
        //             AND IH.UnitId = @UnitId
        //             AND (@StatusId IS NULL OR IH.StatusId = @StatusId)
        //             AND (
        //                     @Search IS NULL
        //                     OR IH.IndentNumber LIKE @Search
        //                     OR IndentType.Code LIKE @Search
        //                 )

        //             GROUP BY 
        //                 IH.Id,
        //                 IH.IndentNumber,
        //                 IH.IndentDate,
        //                 IH.IndentTypeId,
        //                 IndentType.Code,
        //                 IH.UnitId,
        //                 IH.DepartmentId,
        //                 IH.Purpose,
        //                 Status.Code,
        //                 IH.IsActive,
        //                 IH.CreatedDate,
        //                 IH.CreatedByName

        //             ORDER BY IH.Id DESC
        //             OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

        //     ";

        //     var parameters = new
        //     {
        //         UnitId = unitId,
        //         StatusId = statusId,
        //         Search = string.IsNullOrEmpty(searchTerm) ? null : $"%{searchTerm}%",
        //         Offset = (pageNumber - 1) * pageSize,
        //         PageSize = pageSize
        //     };

        //     var data = (await _dbConnection.QueryAsync<PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto, int, PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent.IndentDto>(
        //         sql,
        //         (indent, totalCount) =>
        //         {
        //             indent.ItemCount = indent.ItemCount;
        //             return indent;
        //         },
        //         parameters,
        //         splitOn: "TotalCount"
        //     )).ToList();

        //     var total = data.FirstOrDefault()?.ItemCount ?? 0;

        //     return (data, total);
        // }




    }
}