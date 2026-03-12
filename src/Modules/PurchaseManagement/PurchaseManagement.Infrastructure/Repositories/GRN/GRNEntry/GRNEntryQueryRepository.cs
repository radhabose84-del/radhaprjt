#nullable disable
using System.Data;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPending;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPending;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingDetails;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPendingHeader;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnQCCompletedDetails;
using PurchaseManagement.Domain.Common;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.GRN.GRNEntry
{
    public class GRNEntryQueryRepository : IGRNEntryQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public GRNEntryQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }
        public async Task<string> GetDocumentDirectoryAsync()
        {
            const string query = @"
            SELECT Description            
            FROM Purchase.MiscTypeMaster             
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  IsDeleted=0 and IsActive=1
            ORDER BY ID DESC";
            var parameters = new { MiscTypeCode = MiscEnumEntity.GrnReceivedImage };
            var result = await _dbConnection.QueryAsync<string>(query, parameters);
            return result.FirstOrDefault();
        }



        // public async Task<List<GetGrnPendingDetailsDto>> GetPendingGateEntriesForGrnAsync(
        //     int? GrnId,
        //     bool? IsGrnGenerated,
        //     bool? IsQcGenerated)
        // {
        //     var UnitId = _ipAddressService.GetUnitId() ?? 0;

        //     // Prepare SQL parameters
        //     var parameters = new DynamicParameters();
        //     parameters.Add("UnitId", UnitId);

        //     if (GrnId.HasValue)
        //         parameters.Add("GrnId", GrnId.Value);

        //     if (IsGrnGenerated.HasValue)
        //         parameters.Add("IsGrnGenerated", IsGrnGenerated.Value ? 1 : 0); // Convert bool to bit

        //     if (IsQcGenerated.HasValue)
        //         parameters.Add("IsQcGenerated", IsQcGenerated.Value ? 1 : 0); // Convert bool to bit

        //     // Dynamic PO Method Parameters from MiscEnumEntity
        //         parameters.Add("POMethodType", MiscEnumEntity.POMethod);   // Group Name
        //         parameters.Add("LocalPO", MiscEnumEntity.Local);           // Local
        //         parameters.Add("ImportPO", MiscEnumEntity.Import);  


        //     // Build WHERE clause dynamically
        //     var whereClause = "WHERE A.UnitId = @UnitId";
        //     if (GrnId.HasValue) whereClause += " AND A.Id = @GrnId";
        //     if (IsGrnGenerated.HasValue) whereClause += " AND A.IsGrnGenerated = @IsGrnGenerated";
        //     if (IsQcGenerated.HasValue) whereClause += " AND A.IsQcApproved = @IsQcGenerated";


        //     // var query = $@"
        //     //     WITH GrnSummary AS
        //     //     (
        //     //         SELECT PoId, PoSlNoLocal, SUM(DcQuantity) AS TotalGrnQty
        //     //         FROM Purchase.GrnDetail
        //     //         GROUP BY PoId, PoSlNoLocal
        //     //     ),
        //     //     RankedGrn AS
        //     //     (
        //     //         SELECT
        //     //             A.Id AS GrnId,
        //     //             A.GrnNo,
        //     //             A.GrnDate,
        //     //             A.UnitId,
        //     //             A.GateEntryId,
        //     //             A.PartyId,
        //     //             C.GateEntryNo,
        //     //             C.GateEntryDate,
        //     //             A.InvoiceNo,
        //     //             A.InvoiceDate,
        //     //             A.DcNo,
        //     //             A.DcDate,
        //     //             A.ReceivingWarehouseId,
        //     //             A.Remarks,
        //     //             A.IsGrnGenerated,
        //     //             A.GrnReceivedImage,
        //     //             A.CreatedDate,
        //     //             A.CreatedByName,
        //     //             A.ModifiedDate,
        //     //             A.ModifiedByName,
        //     //             A.QcRemarks,
        //     //             A.QcPersonName,
        //     //             A.QcStatusId,
        //     //             A.QcDate,
        //     //             A.IsQcApproved,
        //     //             A.QcWarehouseId,
        //     //             A.RejectedImage,
        //     //             B.Id,
        //     //             B.GrnId as GrnDetailId,
        //     //             B.PoId,
        //     //             B.PoSlNoLocal,
        //     //             B.PoCategoryId,
        //     //             B.PoMethodId,
        //     //             E.PONumber,
        //     //             B.ItemId,
        //     //             B.OrderQuantity,
        //     //             B.DcQuantity,
        //     //             B.ReceivedQuantity,
        //     //             B.ExpiryDate,
        //     //             B.BatchNumber,
        //     //             B.QcAcceptedQuantity,
        //     //             B.QcRejectedQuantity,
        //     //             B.QcRejectedRemarks,
        //     //             (B.OrderQuantity - ISNULL(gs.TotalGrnQty, 0)) AS PendingQty,
        //     //             ROW_NUMBER() OVER (PARTITION BY B.PoId, B.PoSlNoLocal ORDER BY B.Id) AS rn
        //     //         FROM Purchase.GrnHeader A
        //     //         INNER JOIN Purchase.GrnDetail B ON A.Id = B.GrnId
        //     //         INNER JOIN Purchase.GateEntryHeader C ON A.GateEntryId = C.Id
        //     //         INNER JOIN Purchase.GateEntryDetail D ON C.Id = D.GateEntryHeaderId
        //     //         INNER JOIN Purchase.PurchaseOrderHeader E ON E.Id = D.PoId
        //     //         INNER JOIN Purchase.PurchaseLocalHeader F ON F.PurchaseOrderId = E.Id
        //     //         INNER JOIN Purchase.PurchaseLocalDetail G ON G.PurchaseLocalId = F.Id AND B.PoSlNoLocal = G.ItemSno and B.ItemId = G.ItemId
        //     //         LEFT JOIN GrnSummary gs ON gs.PoId = B.PoId AND gs.PoSlNoLocal = B.PoSlNoLocal
        //     //         {whereClause}
        //     //     )
        //     //     SELECT *
        //     //     FROM RankedGrn
        //     //     WHERE rn = 1
        //     //     ORDER BY PoId, PoSlNoLocal;

        //     var query = $@"
        //     WITH GrnSummary AS
        //                 (
        //                     SELECT 
        //                         PoId, 
        //                         PoSlNoLocal, 
        //                         SUM(DcQuantity) AS TotalGrnQty
        //                     FROM Purchase.GrnDetail
        //                     GROUP BY PoId, PoSlNoLocal
        //                 ),

        //                 RankedGrn AS
        //                 (
        //                     SELECT
        //                         -- GRN HEADER
        //                         A.Id AS GrnId,
        //                         A.GrnNo,
        //                         A.GrnDate,
        //                         A.UnitId,
        //                         A.GateEntryId,
        //                         A.PartyId,
        //                         C.GateEntryNo,
        //                         C.GateEntryDate,
        //                         A.InvoiceNo,
        //                         A.InvoiceDate,
        //                         A.DcNo,
        //                         A.DcDate,
        //                         A.ReceivingWarehouseId,
        //                         A.Remarks,
        //                         A.IsGrnGenerated,
        //                         A.GrnReceivedImage,
        //                         A.CreatedDate,
        //                         A.CreatedByName,
        //                         A.ModifiedDate,
        //                         A.ModifiedByName,
        //                         A.QcRemarks,
        //                         A.QcPersonName,
        //                         A.QcStatusId,
        //                         A.QcDate,
        //                         A.IsQcApproved,
        //                         A.QcWarehouseId,
        //                         A.RejectedImage,

        //                         -- GRN DETAIL
        //                         B.Id AS GrnDetailId,
        //                         B.GrnId,
        //                         B.PoId,
        //                         B.PoSlNoLocal,
        //                         B.PoCategoryId,
        //                         B.PoMethodId,
        //                         E.PONumber,
        //                         B.ItemId,
        //                         B.OrderQuantity,
        //                         B.DcQuantity,
        //                         B.ReceivedQuantity,
        //                         B.ExpiryDate,
        //                         B.BatchNumber,
        //                         B.QcAcceptedQuantity,
        //                         B.QcRejectedQuantity,
        //                         B.QcRejectedRemarks,

        //                         -- Pending Qty Calculation
        //                         (B.OrderQuantity - ISNULL(gs.TotalGrnQty, 0)) AS PendingQty,

        //                         ROW_NUMBER() OVER (PARTITION BY B.PoId, B.PoSlNoLocal ORDER BY B.Id) AS rn

        //                     FROM Purchase.GrnHeader A
        //                     INNER JOIN Purchase.GrnDetail B 
        //                         ON A.Id = B.GrnId

        //                     INNER JOIN Purchase.GateEntryHeader C 
        //                         ON A.GateEntryId = C.Id

        //                     INNER JOIN Purchase.GateEntryDetail D 
        //                         ON C.Id = D.GateEntryHeaderId

        //                     INNER JOIN Purchase.PurchaseOrderHeader E 
        //                         ON E.Id = D.PoId

        //                     -- 🔥 Correct JOIN for PO Method
        //                     INNER JOIN Purchase.MiscMaster MM 
        //                         ON MM.Id = E.POMethodId
        //                     AND MM.Description = @PoMethodEnum  

        //                     -- LOCAL PO 
        //                     LEFT JOIN Purchase.PurchaseLocalHeader F 
        //                         ON E.POCategoryId = 1057
        //                     AND F.PurchaseOrderId = E.Id

        //                     LEFT JOIN Purchase.PurchaseLocalDetail G 
        //                         ON E.POCategoryId = 1057
        //                     AND G.PurchaseLocalId = F.Id
        //                     AND G.ItemSno = B.PoSlNoLocal
        //                     AND G.ItemId = B.ItemId

        //                     -- IMPORT PO 
        //                     LEFT JOIN Purchase.PurchaseOrderImportHeader IH
        //                         ON E.POCategoryId = 1058
        //                     AND IH.PurchaseOrderId = E.Id

        //                     LEFT JOIN Purchase.PurchaseOrderImportDetail ID
        //                         ON E.POCategoryId = 1058
        //                     AND ID.PurchaseHeaderId = IH.Id
        //                     AND ID.ItemId = B.ItemId
        //                     AND ID.ItemSno = B.PoSlNoLocal

        //                     LEFT JOIN GrnSummary gs
        //                         ON gs.PoId = B.PoId
        //                     AND gs.PoSlNoLocal = B.PoSlNoLocal

        //                     {whereClause}
        //                 )

        //                 SELECT *
        //                 FROM RankedGrn
        //                 WHERE rn = 1
        //                 ORDER BY PoId, PoSlNoLocal;



        //     ";

        //     var grnDictionary = new Dictionary<int, GetGrnPendingDetailsDto>();

        //     var result = await _dbConnection.QueryAsync<GetGrnPendingDetailsDto, GetGrnPendingDetailsDto.GetGateEntryPendingDetailsGRNDto, GetGrnPendingDetailsDto>(
        //         query,
        //         (header, detail) =>
        //         {
        //             if (!grnDictionary.TryGetValue(header.GrnId, out var grn))
        //             {
        //                 grn = header;
        //                 grn.GrnDetails = new List<GetGrnPendingDetailsDto.GetGateEntryPendingDetailsGRNDto>();
        //                 grnDictionary.Add(grn.GrnId, grn);
        //             }

        //             if (detail != null)
        //                 grn.GrnDetails.Add(detail);

        //             return grn;
        //         },
        //         parameters,
        //         splitOn: "Id"
        //     );

        //     return grnDictionary.Values.ToList();
        // }

        public async Task<List<GetGrnPendingDetailsDto>> GetPendingGateEntriesForGrnAsync(
                int? GrnId,
                bool? IsGrnGenerated,
                bool? IsQcGenerated)
            {
                var UnitId = _ipAddressService.GetUnitId() ?? 0;

                var parameters = new DynamicParameters();
                parameters.Add("UnitId", UnitId);

                if (GrnId.HasValue)
                    parameters.Add("GrnId", GrnId.Value);

                if (IsGrnGenerated.HasValue)
                    parameters.Add("IsGrnGenerated", IsGrnGenerated.Value ? 1 : 0);

                if (IsQcGenerated.HasValue)
                    parameters.Add("IsQcGenerated", IsQcGenerated.Value ? 1 : 0);

                // Dynamic PO Method Parameters from MiscEnumEntity
                parameters.Add("POMethodType", MiscEnumEntity.POMethod);   // Group Name
                parameters.Add("LocalPO", MiscEnumEntity.Local);           // Local
                parameters.Add("ImportPO", MiscEnumEntity.Import);         // Import

                var whereClause = "WHERE A.UnitId = @UnitId";
                if (GrnId.HasValue) whereClause += " AND A.Id = @GrnId";
                if (IsGrnGenerated.HasValue) whereClause += " AND A.IsGrnGenerated = @IsGrnGenerated";
                if (IsQcGenerated.HasValue) whereClause += " AND A.IsQcApproved = @IsQcGenerated";

                var query = $@"
                WITH POMethodType AS (
                    SELECT Id AS POMethodTypeId 
                    FROM Purchase.MiscTypeMaster 
                    WHERE Description = @POMethodType
                ),
                LocalCategory AS (
                    SELECT Id AS LocalCategoryId 
                    FROM Purchase.MiscMaster 
                    WHERE Description = @LocalPO
                ),
                ImportCategory AS (
                    SELECT Id AS ImportCategoryId 
                    FROM Purchase.MiscMaster 
                    WHERE Description = @ImportPO
                ),
                GrnSummary AS
                (
                    SELECT PoId, PoSlNoLocal, SUM(DcQuantity) AS TotalGrnQty
                    FROM Purchase.GrnDetail
                    GROUP BY PoId, PoSlNoLocal
                ),
                RankedGrn AS
                (
                    SELECT
                        A.Id AS GrnId,
                        A.GrnNo,
                        A.GrnDate,
                        A.UnitId,
                        A.GateEntryId,
                        A.PartyId,
                        C.GateEntryNo,
                        C.GateEntryDate,
                        A.InvoiceNo,
                        A.InvoiceDate,
                        A.DcNo,
                        A.DcDate,
                        A.ReceivingWarehouseId,
                        A.Remarks,
                        A.IsGrnGenerated,
                        A.GrnReceivedImage,
                        A.CreatedDate,
                        A.CreatedByName,
                        A.ModifiedDate,
                        A.ModifiedByName,
                        A.QcRemarks,
                        A.QcPersonName,
                        A.QcStatusId,
                        A.QcDate,
                        A.IsQcApproved,
                        A.QcWarehouseId,
                        A.RejectedImage,

                        -- DETAIL
                        B.Id,
                        B.GrnId as GrnDetailId,
                        B.PoId,
                        B.PoSlNoLocal,
                        B.PoCategoryId,
                        B.PoMethodId,
                        E.PONumber,
                        B.ItemId,
                        B.OrderQuantity,
                        B.DcQuantity,
                        B.ReceivedQuantity,
                        B.ExpiryDate,
                        B.BatchNumber,
                        B.QcAcceptedQuantity,
                        B.QcRejectedQuantity,
                        B.QcRejectedRemarks,

                        (B.OrderQuantity - ISNULL(gs.TotalGrnQty, 0)) AS PendingQty,

                        ROW_NUMBER() OVER (PARTITION BY B.PoId, B.PoSlNoLocal ORDER BY B.Id) AS rn

                    FROM Purchase.GrnHeader A
                    INNER JOIN Purchase.GrnDetail B ON A.Id = B.GrnId
                    INNER JOIN Purchase.GateEntryHeader C ON A.GateEntryId = C.Id
                    INNER JOIN Purchase.GateEntryDetail D ON C.Id = D.GateEntryHeaderId
                    INNER JOIN Purchase.PurchaseOrderHeader E ON E.Id = D.PoId

                    -- ✔ Correct PO Method join (Local / Import)
                    INNER JOIN Purchase.MiscMaster MM 
                        ON MM.Id = E.POMethodId
                    AND MM.MiscTypeId = (SELECT POMethodTypeId FROM POMethodType)

                    -- ✔ LOCAL PO (dynamic)
                    LEFT JOIN LocalCategory LC ON LC.LocalCategoryId = E.PoMethodId
                    LEFT JOIN Purchase.PurchaseLocalHeader F 
                        ON F.PurchaseOrderId = E.Id
                    LEFT JOIN Purchase.PurchaseLocalDetail G
                        ON G.PurchaseLocalId = F.Id
                        AND G.ItemSno = B.PoSlNoLocal
                        AND G.ItemId = B.ItemId

                    -- ✔ IMPORT PO (dynamic)
                    LEFT JOIN ImportCategory IC ON IC.ImportCategoryId = E.PoMethodId
                    LEFT JOIN Purchase.PurchaseOrderImportHeader IH
                        ON IH.PurchaseOrderId = E.Id
                    LEFT JOIN Purchase.PurchaseOrderImportDetail ID
                        ON ID.PurchaseHeaderId = IH.Id
                        AND ID.ItemId = B.ItemId
                        AND ID.ItemSno = B.PoSlNoLocal

                    LEFT JOIN GrnSummary gs 
                        ON gs.PoId = B.PoId 
                    AND gs.PoSlNoLocal = B.PoSlNoLocal

                    {whereClause}
                )

                SELECT *
                FROM RankedGrn
                WHERE rn = 1
                ORDER BY PoId, PoSlNoLocal;
                ";

                var grnDictionary = new Dictionary<int, GetGrnPendingDetailsDto>();

                var result = await _dbConnection.QueryAsync<
                    GetGrnPendingDetailsDto,
                    GetGrnPendingDetailsDto.GetGateEntryPendingDetailsGRNDto,
                    GetGrnPendingDetailsDto>(
                    query,
                    (header, detail) =>
                    {
                        if (!grnDictionary.TryGetValue(header.GrnId, out var grn))
                        {
                            grn = header;
                            grn.GrnDetails = new List<GetGrnPendingDetailsDto.GetGateEntryPendingDetailsGRNDto>();
                            grnDictionary.Add(grn.GrnId, grn);
                        }

                        if (detail != null)
                            grn.GrnDetails.Add(detail);

                        return grn;
                    },
                    parameters,
                    splitOn: "Id"
                );

                return grnDictionary.Values.ToList();
            }


        public async Task<(List<GetGrnQCCompletedDetailsDto>, int)> GetGrnQcCompletedHeader(
         DateTimeOffset? fromDate,
         DateTimeOffset? toDate,
         int PageNumber,
         int PageSize,
         string SearchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;

            var parameters = new DynamicParameters();
            parameters.Add("UnitId", UnitId);
            parameters.Add("Offset", (PageNumber - 1) * PageSize);
            parameters.Add("PageSize", PageSize);

            // WHERE clause
            var whereClause = "WHERE G.UnitId = @UnitId AND D.QcAcceptedQuantity > 0 AND P.Id IS NULL";

            if (fromDate.HasValue)
            {
                whereClause += " AND G.CreatedDate >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
                whereClause += " AND G.CreatedDate <= @ToDate";
                parameters.Add("ToDate", toDateEnd);
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                whereClause += " AND (G.GrnNo LIKE @Search OR G.Id LIKE @Search OR C.GateEntryNo LIKE @Search OR G.PartyId LIKE @Search)";
                parameters.Add("Search", $"%{SearchTerm}%");
            }

            var query = $@"
                    -- =========================================
                    -- 1️⃣ MAIN PAGED RESULT
                    -- =========================================
                    WITH FilteredGrn AS (
                        SELECT
                            G.Id AS GrnId,
                            G.GrnNo,
                            G.GrnDate,
                            G.UnitId,
                            G.GateEntryId,
                            G.PartyId,
                            C.GateEntryNo,
                            C.GateEntryDate,
                            G.InvoiceNo,
                            G.InvoiceDate,
                            G.DcNo,
                            G.DcDate,
                            G.ReceivingWarehouseId,
                            G.Remarks,
                            G.QcRemarks,
                            G.QcStatusId,
                            G.QcPersonName,
                            G.QcDate,
                            G.QcWarehouseId,
                            G.IsQcApproved,
                            G.RejectedImage,
                            ROW_NUMBER() OVER (ORDER BY G.GrnDate DESC) AS RowNum
                        FROM Purchase.GrnHeader G
                        INNER JOIN Purchase.GrnDetail D ON G.Id = D.GrnId
                        INNER JOIN Purchase.GateEntryHeader C ON G.GateEntryId = C.Id
                        LEFT JOIN Purchase.GrnPutAwayRule P 
                            ON D.Id = P.GrnDetailId 
                            AND D.GrnId = P.GrnId 
                            AND D.PoId = P.PoId 
                            AND D.PoSlNoLocal = P.PoSlNoLocal 
                            AND D.ItemId = P.ItemId
                        {whereClause}
                        GROUP BY
                            G.Id, G.GrnNo, G.GrnDate, G.UnitId, G.GateEntryId, G.PartyId,
                            C.GateEntryNo, C.GateEntryDate, G.InvoiceNo, G.InvoiceDate,
                            G.DcNo, G.DcDate, G.ReceivingWarehouseId, G.Remarks,
                            G.QcRemarks, G.QcStatusId, G.QcPersonName, G.QcDate,
                            G.QcWarehouseId, G.IsQcApproved, G.RejectedImage
                    )
                    SELECT *
                    FROM FilteredGrn
                    ORDER BY GrnDate DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                    -- =========================================
                    -- 2️⃣ TOTAL COUNT
                    -- =========================================
                    WITH FilteredGrn AS (
                        SELECT G.Id
                        FROM Purchase.GrnHeader G
                        INNER JOIN Purchase.GrnDetail D ON G.Id = D.GrnId
                        INNER JOIN Purchase.GateEntryHeader C ON G.GateEntryId = C.Id
                        LEFT JOIN Purchase.GrnPutAwayRule P 
                            ON D.Id = P.GrnDetailId 
                            AND D.GrnId = P.GrnId 
                            AND D.PoId = P.PoId 
                            AND D.PoSlNoLocal = P.PoSlNoLocal 
                            AND D.ItemId = P.ItemId
                        {whereClause}
                        GROUP BY G.Id
                    )
                    SELECT COUNT(*) AS TotalCount FROM FilteredGrn;
                ";

            using var multi = await _dbConnection.QueryMultipleAsync(query, parameters);

            var grnList = (await multi.ReadAsync<GetGrnQCCompletedDetailsDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();

            return (grnList, totalCount);
        }



        public async Task<(List<GetGrnPendingHeaderDto>, int)> GetPendingGrnHeaderAsync(
               DateTimeOffset? fromDate,
               DateTimeOffset? toDate,
               bool? IsGrnGenerated,
               bool? IsQcGenerated,
               int PageNumber,
               int PageSize,
               string SearchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;

            var parameters = new DynamicParameters();
            parameters.Add("UnitId", UnitId);

            // WHERE clause
            var whereClause = "WHERE A.UnitId = @UnitId";

            if (fromDate.HasValue)
            {
                whereClause += " AND A.CreatedDate >= @FromDate";
                parameters.Add("FromDate", fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var toDateEnd = toDate.Value.Date.AddDays(1).AddTicks(-1); // end of day
                whereClause += " AND A.CreatedDate <= @ToDate";
                parameters.Add("ToDate", toDateEnd);
            }

            if (IsGrnGenerated.HasValue)
            {
                whereClause += " AND A.IsGrnGenerated = @IsGrnGenerated";
                parameters.Add("IsGrnGenerated", IsGrnGenerated.Value ? 1 : 0);
            }

            if (IsQcGenerated.HasValue)
            {
                whereClause += " AND A.IsQcApproved = @IsQcGenerated";
                parameters.Add("IsQcGenerated", IsQcGenerated.Value ? 1 : 0);
            }

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                whereClause += " AND (A.GrnNo LIKE @SearchTerm OR A.Id LIKE @SearchTerm OR C.GateEntryNo LIKE @SearchTerm)";
                parameters.Add("SearchTerm", $"%{SearchTerm}%");
            }

            // Pagination parameters
            var offset = (PageNumber - 1) * PageSize;
            parameters.Add("Offset", offset);
            parameters.Add("PageSize", PageSize);

            // Main query with pagination
            var query = $@"
                SELECT
                    A.Id AS GrnId,
                    A.GrnNo,
                    A.GrnDate,
                    A.UnitId,
                    A.GateEntryId,
                    A.PartyId,
                    C.GateEntryNo,
                    C.GateEntryDate,
                    A.InvoiceNo,
                    A.InvoiceDate,
                    A.DcNo,
                    A.DcDate,
                    A.ReceivingWarehouseId,
                    A.Remarks,
                    A.IsGrnGenerated,
                    A.GrnReceivedImage,
                    A.CreatedDate,
                    A.CreatedByName,
                    A.ModifiedDate,
                    A.ModifiedByName,
                    A.QcRemarks,
                    A.QcStatusId,
                    A.QcPersonName,
                    A.QcDate,
                    A.QcWarehouseId,
                    A.IsQcApproved,
                    A.RejectedImage
                FROM Purchase.GrnHeader A
                INNER JOIN Purchase.GateEntryHeader C ON A.GateEntryId = C.Id
                {whereClause}
                ORDER BY A.CreatedDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT COUNT(1)
                FROM Purchase.GrnHeader A
                INNER JOIN Purchase.GateEntryHeader C ON A.GateEntryId = C.Id
                {whereClause};
            ";

            using (var multi = await _dbConnection.QueryMultipleAsync(query, parameters))
            {
                var data = (await multi.ReadAsync<GetGrnPendingHeaderDto>()).ToList();
                var totalCount = await multi.ReadFirstAsync<int>();
                return (data, totalCount);
            }
        }


        public async Task<List<GetGateEntryPendingPoDto>> GetPendingPoAsync(int partyId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            // var query = @"
            //         SELECT 
            //         gd.GateEntryHeaderId as GateEntryId,
            //         gd.PoId,
            //         poh.PONumber,
            //         poh.PODate,
            //         poh.VendorId AS PartyId
            //     FROM Purchase.GateEntryHeader geh
            //     INNER JOIN Purchase.GateEntryDetail gd
            //         ON geh.Id = gd.GateEntryHeaderId
            //     INNER JOIN Purchase.PurchaseOrderHeader poh
            //         ON gd.PoId = poh.Id
            //     WHERE geh.IsDeleted = 0
            //     AND geh.IsActive = 1
            //     AND geh.UnitId = @UnitId
            //     AND poh.VendorId = @PartyId
            //     AND NOT EXISTS (
            //         SELECT 1
            //         FROM Purchase.GrnHeader gh
            //         INNER JOIN Purchase.GrnDetail gd2
            //             ON gh.Id = gd2.GrnId
            //         WHERE gh.UnitId = geh.UnitId
            //             AND gh.GateEntryId = geh.Id
            //             AND gd2.PoId = gd.PoId
            //     )
            //     ORDER BY gd.GateEntryHeaderId, gd.PoId;

            //         ";

            var query = @"
                        SELECT 
                        gd.GateEntryHeaderId AS GateEntryId,
                        gd.PoId,
                        poh.PONumber,
                        poh.PODate,
                        poh.VendorId AS PartyId
                    FROM Purchase.GateEntryHeader geh
                    INNER JOIN Purchase.GateEntryDetail gd
                        ON geh.Id = gd.GateEntryHeaderId
                    INNER JOIN Purchase.PurchaseOrderHeader poh
                        ON gd.PoId = poh.Id

                     
                    LEFT JOIN Purchase.PurchaseLocalHeader plh 
                        ON poh.Id = plh.PurchaseOrderId
                    LEFT JOIN Purchase.PurchaseOrderImportHeader poih
                        ON poh.Id = poih.PurchaseOrderId

                    WHERE geh.IsDeleted = 0
                        AND geh.IsActive = 1
                        AND geh.UnitId = @UnitId
                        AND poh.VendorId = @PartyId
                        And poh.IsDeleted = 0


         
                        AND NOT EXISTS (
                            SELECT 1
                            FROM Purchase.GrnHeader gh
                            INNER JOIN Purchase.GrnDetail gd2
                                ON gh.Id = gd2.GrnId
                            WHERE gh.UnitId = geh.UnitId
                            AND gh.GateEntryId = geh.Id
                            AND gd2.PoId = gd.PoId
                        )

                    ORDER BY gd.GateEntryHeaderId, gd.PoId;
            ";

            var result = await _dbConnection.QueryAsync<GetGateEntryPendingPoDto>(
                query,
                new { PartyId = partyId, UnitId = UnitId }
            );

            return result.AsList();
        }

        public async Task<List<GetGateEntryPendingDto>> GetPendingPoGateAsync(int partyId, int poId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            // var query = @"
            //         WITH GrnSummary AS
            //         (
            //             SELECT 
            //                 PoId,
            //                 ItemId,
            //                 PoSlNoLocal,
            //                 SUM(DcQuantity) AS TotalGrnQty
            //             FROM Purchase.GrnDetail
            //             GROUP BY PoId, ItemId, PoSlNoLocal
            //         )
            //         SELECT 
            //             DISTINCT
            //             geh.Id AS GateEntryId,
            //             ged.PoId,
            //             poh.PONumber,
            //             geh.GateEntryNo,
            //             geh.GateEntryDate
            //         FROM Purchase.GateEntryHeader geh
            //         INNER JOIN Purchase.GateEntryDetail ged ON geh.Id = ged.GateEntryHeaderId
            //         INNER JOIN Purchase.PurchaseOrderHeader poh ON ged.PoId = poh.Id
            //         INNER JOIN Purchase.PurchaseLocalHeader polh ON poh.Id = polh.PurchaseOrderId
            //         INNER JOIN Purchase.PurchaseLocalDetail pod ON polh.Id = pod.PurchaseLocalId
            //         LEFT JOIN GrnSummary gs
            //             ON gs.PoId = ged.PoId
            //             AND gs.ItemId = pod.ItemId
            //             AND gs.PoSlNoLocal = pod.ItemSno
            //         WHERE geh.IsDeleted = 0
            //         AND geh.IsActive = 1
            //         AND geh.UnitId = @UnitId
            //         AND poh.VendorId = @PartyId
            //         And ged.PoId=@PoId
            //         AND NOT EXISTS (
            //             SELECT 1
            //             FROM Purchase.GrnHeader gh
            //             WHERE gh.GateEntryId = geh.Id AND gh.UnitId = @UnitId
            //         )
            //         ORDER BY ged.PoId DESC;
            //         ";

            var query = @"
            WITH GrnSummary AS
            (
                SELECT 
                    PoId,
                    ItemId,
                    PoSlNoLocal,
                    SUM(DcQuantity) AS TotalGrnQty
                FROM Purchase.GrnDetail
                GROUP BY PoId, ItemId, PoSlNoLocal
            )
            SELECT DISTINCT
                geh.Id AS GateEntryId,
                ged.PoId,
                poh.PONumber,
                geh.GateEntryNo,
                geh.GateEntryDate
            FROM Purchase.GateEntryHeader geh
            INNER JOIN Purchase.GateEntryDetail ged 
                ON geh.Id = ged.GateEntryHeaderId
            INNER JOIN Purchase.PurchaseOrderHeader poh 
                ON ged.PoId = poh.Id

            LEFT JOIN Purchase.PurchaseLocalHeader polh 
                ON poh.Id = polh.PurchaseOrderId
            LEFT JOIN Purchase.PurchaseLocalDetail pod_local
                ON polh.Id = pod_local.PurchaseLocalId

     
            LEFT JOIN Purchase.PurchaseOrderImportHeader poih 
                ON poh.Id = poih.PurchaseOrderId
            LEFT JOIN Purchase.PurchaseOrderImportDetail pod_imp
                ON poih.Id = pod_imp.PurchaseHeaderId

    
            LEFT JOIN GrnSummary gs
                ON gs.PoId = ged.PoId
                AND gs.ItemId = COALESCE(pod_local.ItemId, pod_imp.ItemId)
                AND gs.PoSlNoLocal = COALESCE(pod_local.ItemSno, pod_imp.ItemSno)

            WHERE geh.IsDeleted = 0
            AND geh.IsActive = 1
            AND geh.UnitId = @UnitId
            AND poh.VendorId = @PartyId
            AND ged.PoId = @PoId
            AND poh.IsDeleted = 0
            
            -- ✔ Keep same NOT EXISTS logic
            AND NOT EXISTS (
                    SELECT 1
                    FROM Purchase.GrnHeader gh
                    WHERE gh.GateEntryId = geh.Id 
                    AND gh.UnitId = @UnitId
            )

            ORDER BY ged.PoId DESC;

            ";

            var result = await _dbConnection.QueryAsync<GetGateEntryPendingDto>(
                query,
                new { PartyId = partyId, UnitId = UnitId, PoId = poId }
            );

            return result.AsList();
        }


        public async Task<List<GetGrnPendingDto>> GetPendingPoGrnAsync(int partyId, int poId, int gateEntryId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            //     var query = @"
            // WITH GrnSummary AS
            // (
            //     SELECT 
            //         PoId,
            //         ItemId,
            //         PoSlNoLocal,
            //         SUM(DcQuantity) AS TotalGrnQty
            //     FROM Purchase.GrnDetail
            //     GROUP BY PoId, ItemId, PoSlNoLocal
            // )
            // SELECT 
            //     geh.Id AS GateEntryId,
            //     geh.UnitId,
            //     poh.VendorId AS PartyId,
            //     geh.GateEntryNo,
            //     geh.GateEntryDate,
            //     polh.IsPartialReceiptAllowed,
            //     ged.PoId,
            //     poh.PONumber,
            //     poh.POCategoryId,
            // 	poh.POMethodId,
            //     pod.ItemSno AS PoSlNo,
            //     pod.ItemId,
            //     pod.Quantity AS OrderQuantity,
            //     ISNULL(gs.TotalGrnQty, 0) AS TotalGrnQty,
            //     pod.Quantity - ISNULL(gs.TotalGrnQty, 0) AS PendingQty
            // FROM Purchase.GateEntryHeader geh
            // INNER JOIN Purchase.GateEntryDetail ged ON geh.Id = ged.GateEntryHeaderId
            // INNER JOIN Purchase.PurchaseOrderHeader poh ON ged.PoId = poh.Id
            // INNER JOIN Purchase.PurchaseLocalHeader polh ON poh.Id = polh.PurchaseOrderId
            // INNER JOIN Purchase.PurchaseLocalDetail pod ON polh.Id = pod.PurchaseLocalId
            // LEFT JOIN GrnSummary gs
            //     ON gs.PoId = ged.PoId
            //     AND gs.ItemId = pod.ItemId
            //     AND gs.PoSlNoLocal = pod.ItemSno
            // WHERE geh.IsDeleted = 0
            // AND geh.IsActive = 1
            // AND geh.UnitId = @UnitId
            // AND poh.VendorId = @PartyId
            // AND ged.PoId = @PoId
            // AND geh.Id = @GateEntryId
            // AND NOT EXISTS (
            //     SELECT 1
            //     FROM Purchase.GrnHeader gh
            //     WHERE gh.GateEntryId = geh.Id AND gh.UnitId = @UnitId
            // )
            // ORDER BY ged.PoId DESC;
            var query = @"
                        WITH GrnSummary AS
                        (
                            SELECT 
                                PoId,
                                ItemId,
                                PoSlNoLocal,
                                SUM(DcQuantity) AS TotalGrnQty
                            FROM Purchase.GrnDetail
                            GROUP BY PoId, ItemId, PoSlNoLocal
                        )
                        SELECT 
                            geh.Id AS GateEntryId,
                            geh.UnitId,
                            poh.VendorId AS PartyId,
                            geh.GateEntryNo,
                            geh.GateEntryDate,

                            -- Local PO field (kept as-is)
                            polh.IsPartialReceiptAllowed,

                            ged.PoId,
                            poh.PONumber,
                            poh.POCategoryId,
                            poh.POMethodId,

                            -- Use Local if available, else Import
                            COALESCE(pod.ItemSno, podi.ItemSno) AS PoSlNo,
                            COALESCE(pod.ItemId, podi.ItemId) AS ItemId,
                            COALESCE(pod.Quantity, podi.Quantity) AS OrderQuantity,

                            ISNULL(gs.TotalGrnQty, 0) AS TotalGrnQty,
                            COALESCE(pod.Quantity, podi.Quantity) - ISNULL(gs.TotalGrnQty, 0) AS PendingQty

                        FROM Purchase.GateEntryHeader geh
                        INNER JOIN Purchase.GateEntryDetail ged 
                            ON geh.Id = ged.GateEntryHeaderId
                        INNER JOIN Purchase.PurchaseOrderHeader poh 
                            ON ged.PoId = poh.Id

                        -- Existing Local PO joins (unchanged)
                        LEFT JOIN Purchase.PurchaseLocalHeader polh 
                            ON poh.Id = polh.PurchaseOrderId
                        LEFT JOIN Purchase.PurchaseLocalDetail pod 
                            ON polh.Id = pod.PurchaseLocalId

                        --  Added Import PO joins (does NOT change Local PO logic)
                        LEFT JOIN Purchase.PurchaseOrderImportHeader poih
                            ON poh.Id = poih.PurchaseOrderId
                        LEFT JOIN Purchase.PurchaseOrderImportDetail podi
                            ON poih.Id = podi.PurchaseHeaderId

                        -- Grn Summary join (support both Local & Import)
                        LEFT JOIN GrnSummary gs
                            ON gs.PoId = ged.PoId
                            AND gs.ItemId = COALESCE(pod.ItemId, podi.ItemId)
                            AND gs.PoSlNoLocal = COALESCE(pod.ItemSno, podi.ItemSno)

                        WHERE geh.IsDeleted = 0
                        AND geh.IsActive = 1
                        AND geh.UnitId = @UnitId
                        AND poh.VendorId = @PartyId
                        AND ged.PoId = @PoId
                        AND geh.Id = @GateEntryId
                        AND poh.IsDeleted = 0

                        -- Keep same NOT EXISTS logic
                        AND NOT EXISTS (
                                SELECT 1
                                FROM Purchase.GrnHeader gh
                                WHERE gh.GateEntryId = geh.Id AND gh.UnitId = @UnitId
                        )

                        ORDER BY ged.PoId DESC;

                ";

            var grnDict = new Dictionary<int, GetGrnPendingDto>();

            var result = await _dbConnection.QueryAsync<GetGrnPendingDto, GetGrnPendingDto.GetGrnPendingDetailsGateDto, GetGrnPendingDto>(
                query,
                (header, detail) =>
                {
                    if (!grnDict.TryGetValue(header.GateEntryId, out var grn))
                    {
                        grn = header;
                        grn.GrnDetails = new List<GetGrnPendingDto.GetGrnPendingDetailsGateDto>();
                        grnDict.Add(grn.GateEntryId, grn);
                    }

                    if (detail != null)
                    {
                        grn.GrnDetails.Add(detail);
                    }

                    return grn;
                },
                new { PartyId = partyId, UnitId = UnitId, PoId = poId, GateEntryId = gateEntryId },
                splitOn: "PoId"
            );

            return grnDict.Values.ToList();
        }


        public async Task<List<ValidateToleranceDto>> ValidateToleranceQuantity(int partyId, int poId, int Poslno, int ItemId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            // var query = @"
            //         WITH GrnSummary AS
            //         (
            //             SELECT 
            //                 PoId,
            //                 ItemId,
            //                 PoSlNoLocal,
            //                 SUM(DcQuantity) AS TotalGrnQty
            //             FROM Purchase.GrnDetail
            //             GROUP BY PoId, ItemId, PoSlNoLocal
            //         )
            //         SELECT 
            //             ged.PoId,
            //             pod.ItemSno AS PoSlNo,
            //             pod.ItemId,
            //             pod.Quantity AS OrderQuantity,
            //             ISNULL(gs.TotalGrnQty, 0) AS TotalGrnQty,
            //             pod.Quantity - ISNULL(gs.TotalGrnQty, 0) AS PendingQty,
            //             polh.IsPartialReceiptAllowed
            //         FROM Purchase.GateEntryHeader geh
            //         INNER JOIN Purchase.GateEntryDetail ged ON geh.Id = ged.GateEntryHeaderId
            //         INNER JOIN Purchase.PurchaseOrderHeader poh ON ged.PoId = poh.Id
            //         INNER JOIN Purchase.PurchaseLocalHeader polh ON poh.Id = polh.PurchaseOrderId
            //         INNER JOIN Purchase.PurchaseLocalDetail pod ON polh.Id = pod.PurchaseLocalId
            //         LEFT JOIN GrnSummary gs
            //             ON gs.PoId = ged.PoId
            //             AND gs.ItemId = pod.ItemId
            //             AND gs.PoSlNoLocal = pod.ItemSno
            //         WHERE geh.IsDeleted = 0
            //         AND geh.IsActive = 1
            //         AND geh.UnitId = @UnitId
            //         AND poh.VendorId = @PartyId
            //         AND ged.PoId = @PoId
            //         And pod.ItemSno= @Poslno
            //         And Poh.IsDeleted = 0
            //         AND NOT EXISTS (
            //             SELECT 1
            //             FROM Purchase.GrnHeader gh
            //             WHERE gh.GateEntryId = geh.Id AND gh.UnitId = @UnitId
            //         )
            //         ORDER BY ged.PoId DESC;
            var query = @"
                        WITH GrnSummary AS
                        (
                            SELECT 
                                PoId,
                                ItemId,
                                PoSlNoLocal,
                                SUM(DcQuantity) AS TotalGrnQty
                            FROM Purchase.GrnDetail
                            GROUP BY PoId, ItemId, PoSlNoLocal
                        )
                        SELECT 
                            ged.PoId,
                            pod.ItemSno AS PoSlNo,
                            pod.ItemId,
                            pod.Quantity AS OrderQuantity,
                            ISNULL(gs.TotalGrnQty, 0) AS TotalGrnQty,
                            pod.Quantity - ISNULL(gs.TotalGrnQty, 0) AS PendingQty,
                            polh.IsPartialReceiptAllowed
                        FROM Purchase.GateEntryHeader geh
                        INNER JOIN Purchase.GateEntryDetail ged 
                            ON geh.Id = ged.GateEntryHeaderId
                        INNER JOIN Purchase.PurchaseOrderHeader poh 
                            ON ged.PoId = poh.Id

                        -- Local PO
                        INNER JOIN Purchase.PurchaseLocalHeader polh 
                            ON poh.Id = polh.PurchaseOrderId
                        INNER JOIN Purchase.PurchaseLocalDetail pod 
                            ON polh.Id = pod.PurchaseLocalId

                        -- Import PO (added without affecting logic)
                        LEFT JOIN Purchase.PurchaseOrderImportHeader imh 
                            ON poh.Id = imh.PurchaseOrderId
                        LEFT JOIN Purchase.PurchaseOrderImportDetail imd 
                            ON imh.Id = imd.PurchaseHeaderId
                            AND imd.ItemSno = pod.ItemSno   -- align line numbers
                            AND imd.ItemId = pod.ItemId

                        LEFT JOIN GrnSummary gs
                            ON gs.PoId = ged.PoId
                            AND gs.ItemId = pod.ItemId
                            AND gs.PoSlNoLocal = pod.ItemSno

                        WHERE geh.IsDeleted = 0
                        AND geh.IsActive = 1
                        AND geh.UnitId = @UnitId
                        AND poh.VendorId = @PartyId
                        AND ged.PoId = @PoId
                        AND pod.ItemSno= @PoSlNo
                        AND poh.IsDeleted = 0
                        AND NOT EXISTS (
                                SELECT 1
                                FROM Purchase.GrnHeader gh
                                WHERE gh.GateEntryId = geh.Id 
                                AND gh.UnitId = @UnitId
                            )
                        ORDER BY ged.PoId DESC;

                                ";

            var result = await _dbConnection.QueryAsync<ValidateToleranceDto>(
                query,
                new { PartyId = partyId, UnitId = UnitId, PoId = poId, Poslno = Poslno, ItemId = ItemId }
            );

            return result.AsList();
        }



        public async Task<List<GetGrnQCCompletedDto>> GetGrnQcCompletedDetails(int? GrnId, int? ItemId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;

            var parameters = new DynamicParameters();
            parameters.Add("UnitId", UnitId);

            if (GrnId.HasValue)
                parameters.Add("GrnId", GrnId.Value);

            if (ItemId.HasValue)
                parameters.Add("ItemId", ItemId.Value);

            var whereClause = "WHERE A.UnitId = @UnitId";
            if (GrnId.HasValue)
                whereClause += " AND A.Id = @GrnId";

            if (ItemId.HasValue && ItemId.Value > 0)
            {
                whereClause += " AND B.ItemId = @ItemId ";
                parameters.Add("ItemId", ItemId.Value);
            }
                 // Dynamic PO Method Parameters from MiscEnumEntity
                parameters.Add("POMethodType", MiscEnumEntity.POMethod);   // Group Name
                parameters.Add("LocalPO", MiscEnumEntity.Local);           // Local
                parameters.Add("ImportPO", MiscEnumEntity.Import); 

            // var query = $@"
            // WITH GrnSummary AS
            // (
            //     SELECT PoId, PoSlNoLocal, SUM(DcQuantity) AS TotalGrnQty
            //     FROM Purchase.GrnDetail
            //     GROUP BY PoId, PoSlNoLocal
            // ),
            // RankedGrn AS
            // (
            //     SELECT
            //         A.Id AS GrnId,
            //         A.GrnNo,
            //         A.GrnDate,
            //         A.UnitId,
            //         A.GateEntryId,
            //         A.PartyId,
            //         C.GateEntryNo,
            //         C.GateEntryDate,
            //         A.InvoiceNo,
            //         A.InvoiceDate,
            //         A.DcNo,
            //         A.DcDate,
            //         A.ReceivingWarehouseId,
            //         A.Remarks,
            //         A.IsGrnGenerated,
            //         A.GrnReceivedImage,
            //         A.CreatedDate,
            //         A.CreatedByName,
            //         A.ModifiedDate,
            //         A.ModifiedByName,
            //         A.QcRemarks,
            //         A.QcPersonName,
            //         A.QcStatusId,
            //         A.QcDate,
            //         A.IsQcApproved,
            //         A.QcWarehouseId,
            //         A.RejectedImage,
            //         B.Id as GrnDetailId,
            //         B.PoId,
            //         B.PoSlNoLocal,
            //         B.PoCategoryId,
            //         B.PoMethodId,
            //         E.PONumber,
            //         B.ItemId,
            //         B.OrderQuantity,
            //         B.DcQuantity,
            //         B.ReceivedQuantity,
            //         B.ExpiryDate,
            //         B.BatchNumber,
            //         B.QcAcceptedQuantity,
            //         G.UnitPrice,
            //         B.QcRejectedQuantity,
            //         B.QcRejectedRemarks,
            //         (B.OrderQuantity - ISNULL(gs.TotalGrnQty, 0)) AS PendingQty,
            //         ROW_NUMBER() OVER (PARTITION BY B.PoId, B.PoSlNoLocal ORDER BY B.Id) AS rn
            //     FROM Purchase.GrnHeader A
            //     INNER JOIN Purchase.GrnDetail B ON A.Id = B.GrnId
            //     INNER JOIN Purchase.GateEntryHeader C ON A.GateEntryId = C.Id
            //     INNER JOIN Purchase.GateEntryDetail D ON C.Id = D.GateEntryHeaderId
            //     INNER JOIN Purchase.PurchaseOrderHeader E ON E.Id = D.PoId
            //     INNER JOIN Purchase.PurchaseLocalHeader F ON F.PurchaseOrderId = E.Id
            //     INNER JOIN Purchase.PurchaseLocalDetail G 
            //         ON G.PurchaseLocalId = F.Id 
            //         AND B.PoSlNoLocal = G.ItemSno
            //     LEFT JOIN GrnSummary gs 
            //         ON gs.PoId = B.PoId 
            //         AND gs.PoSlNoLocal = B.PoSlNoLocal
            //     LEFT JOIN Purchase.GrnPutAwayRule P
            //         ON P.GrnId = A.Id 
            //         AND P.PoId = B.PoId 
            //         AND P.PoSlNoLocal = B.PoSlNoLocal 
            //         AND P.ItemId = B.ItemId
            //         AND E.IsDeleted = 0
            //     {whereClause}
            //     AND B.QcAcceptedQuantity > 0
            //     AND P.Id IS NULL  -- ✅ Exclude already processed for PutAway
            // )
            // SELECT *
            // FROM RankedGrn
            // WHERE rn = 1
            // ORDER BY PoId, PoSlNoLocal;
            var query = $@"
                        ;WITH 
                        POMethodType AS (
                            SELECT Id AS POMethodTypeId 
                            FROM Purchase.MiscTypeMaster 
                            WHERE Description = @POMethodType
                        ),
                        LocalPO AS (
                            SELECT Id AS LocalPOId 
                            FROM Purchase.MiscMaster 
                            WHERE Description = @LocalPO
                        ),
                        ImportPO AS (
                            SELECT Id AS ImportPOId 
                            FROM Purchase.MiscMaster 
                            WHERE Description = @ImportPO
                        ),
                        GrnSummary AS (
                            SELECT PoId, PoSlNoLocal, SUM(DcQuantity) AS TotalGrnQty
                            FROM Purchase.GrnDetail
                            GROUP BY PoId, PoSlNoLocal
                        ),
                        RankedGrn AS (
                            SELECT
                                A.Id AS GrnId,
                                A.GrnNo,
                                A.GrnDate,
                                A.UnitId,
                                A.GateEntryId,
                                A.PartyId,

                                C.GateEntryNo,
                                C.GateEntryDate,

                                A.InvoiceNo,
                                A.InvoiceDate,
                                A.DcNo,
                                A.DcDate,
                                A.ReceivingWarehouseId,
                                A.Remarks,
                                A.IsGrnGenerated,
                                A.GrnReceivedImage,
                                A.CreatedDate,
                                A.CreatedByName,
                                A.ModifiedDate,
                                A.ModifiedByName,
                                A.QcRemarks,
                                A.QcPersonName,
                                A.QcStatusId,
                                A.QcDate,
                                A.IsQcApproved,
                                A.QcWarehouseId,
                                A.RejectedImage,

                                B.Id AS GrnDetailId,
                                B.PoId,
                                B.PoSlNoLocal,
                                E.PONumber,
                                B.ItemId,
                                B.OrderQuantity,
                                B.DcQuantity,
                                B.ReceivedQuantity,
                                B.QcAcceptedQuantity,

                                ISNULL(
                                    CASE 
                                        WHEN E.PoMethodId = (SELECT LocalPOId FROM LocalPO)
                                            THEN G.UnitPrice
                                        WHEN E.PoMethodId = (SELECT ImportPOId FROM ImportPO)
                                            THEN ID.UnitPrice
                                        ELSE 0
                                    END, 0
                                ) AS UnitPrice,

                                ROW_NUMBER() OVER (PARTITION BY B.PoId, B.PoSlNoLocal ORDER BY B.Id) AS rn
                            FROM Purchase.GrnHeader A
                            INNER JOIN Purchase.GrnDetail B ON A.Id = B.GrnId

                            INNER JOIN Purchase.GateEntryHeader C 
                                ON A.GateEntryId = C.Id

                            INNER JOIN Purchase.GateEntryDetail D 
                                ON C.Id = D.GateEntryHeaderId

                            INNER JOIN Purchase.PurchaseOrderHeader E 
                                ON E.Id = B.PoId

                            LEFT JOIN Purchase.PurchaseLocalHeader F 
                                ON F.PurchaseOrderId = E.Id

                            LEFT JOIN Purchase.PurchaseLocalDetail G
                                ON G.PurchaseLocalId = F.Id
                                AND G.ItemId = B.ItemId
                                AND G.ItemSno = B.PoSlNoLocal

                            LEFT JOIN Purchase.PurchaseOrderImportHeader IH 
                                ON IH.PurchaseOrderId = E.Id

                            LEFT JOIN Purchase.PurchaseOrderImportDetail ID
                                ON ID.PurchaseHeaderId = IH.Id
                                AND ID.ItemId = B.ItemId
                                AND ID.ItemSno = B.PoSlNoLocal

                            LEFT JOIN Purchase.GrnPutAwayRule P
                                ON P.GrnId = A.Id
                                AND P.PoId = B.PoId
                                AND P.PoSlNoLocal = B.PoSlNoLocal
                                AND P.ItemId = B.ItemId

                            {whereClause}
                                AND E.IsDeleted = 0
                                AND B.QcAcceptedQuantity > 0
                                AND P.Id IS NULL
                        )

                        SELECT *
                        FROM RankedGrn
                        WHERE rn = 1
                        ORDER BY PoId, PoSlNoLocal;
                    ";

                        


        

            var grnDictionary = new Dictionary<int, GetGrnQCCompletedDto>();

            var result = await _dbConnection.QueryAsync<GetGrnQCCompletedDto, GetGrnQCCompletedDto.GetGrnQCCompletedDtoDetails, GetGrnQCCompletedDto>(
                query,
                (header, detail) =>
                {
                    if (!grnDictionary.TryGetValue(header.GrnId, out var grn))
                    {
                        grn = header;
                        grn.GrnDetails = new List<GetGrnQCCompletedDto.GetGrnQCCompletedDtoDetails>();
                        grnDictionary.Add(grn.GrnId, grn);
                    }

                    if (detail != null)
                        grn.GrnDetails.Add(detail);

                    return grn;
                },
                parameters,
                splitOn: "GrnDetailId"
            );

            return grnDictionary.Values.ToList();
        }
            public async Task<decimal?> GetUnitPriceAsync(int poId, int itemId, int poSlNoLocal)
            {
                var UnitId = _ipAddressService.GetUnitId() ?? 0;

                var parameters = new DynamicParameters();
                parameters.Add("PoId", poId);
                parameters.Add("ItemId", itemId);
                parameters.Add("PoSlNoLocal", poSlNoLocal);
                parameters.Add("UnitId", UnitId);

                // Dynamic PO values from MiscEnumEntity
                parameters.Add("LocalPO", MiscEnumEntity.Local);
                parameters.Add("ImportPO", MiscEnumEntity.Import);

                const string query = @"
                    SELECT TOP 1
                        CASE
                            WHEN A.POMethodId = LM.Id THEN LCD.UnitPrice
                            WHEN A.POMethodId = IM.Id THEN ID.UnitPrice
                            ELSE 0
                        END AS UnitPrice
                    FROM Purchase.PurchaseOrderHeader A

                    LEFT JOIN Purchase.MiscMaster LM
                        ON LM.Id = A.POMethodId AND LM.Description = @LocalPO

                    LEFT JOIN Purchase.MiscMaster IM
                        ON IM.Id = A.POMethodId AND IM.Description = @ImportPO

                    LEFT JOIN Purchase.PurchaseLocalHeader LH
                        ON LH.PurchaseOrderId = A.Id

                    LEFT JOIN Purchase.PurchaseLocalDetail LCD
                        ON LCD.PurchaseLocalId = LH.Id
                        AND LCD.ItemId = @ItemId
                        AND LCD.ItemSno = @PoSlNoLocal

                    LEFT JOIN Purchase.PurchaseOrderImportHeader IH
                        ON IH.PurchaseOrderId = A.Id

                    LEFT JOIN Purchase.PurchaseOrderImportDetail ID
                        ON ID.PurchaseHeaderId = IH.Id
                        AND ID.ItemId = @ItemId
                        AND ID.ItemSno = @PoSlNoLocal

                    WHERE A.Id = @PoId
                    AND A.UnitId = @UnitId
                    AND A.IsDeleted = 0;
                ";

                return await _dbConnection.QueryFirstOrDefaultAsync<decimal?>(query, parameters);
            }

        public async Task<List<PoValueDetailsDto>> GetPoOtherDetails(
            int PoId, 
            int PoSlNoLocal, 
            int PoCategoryId, 
            int PoMethodId, 
            int ItemId)
        {
            var sql = @"
                SELECT 
                    A.Id AS PoId,
                    A.POCategoryId,
                    A.POMethodId,
                    A.MiscCharges,
                    ISNULL(C.Quantity, E.Quantity) AS Quantity,
                    ISNULL(C.ItemId, E.ItemId) AS ItemId,
                    ISNULL(C.UnitPrice, E.UnitPrice) AS UnitPrice,
                    ISNULL(C.DiscountValue, 0.00) AS DiscountValue,
                    ISNULL(C.PandFCharge, 0.00) AS PandFCharge,
                    ISNULL(C.OtherCharge, E.OtherCharges) AS OtherCharge,
                    ISNULL(0.00, E.FreightAmount) AS FreightAmount,
                    ISNULL(0.00, E.InsuranceAmount) AS InsuranceAmount,
                    ISNULL(0.00, E.CIFValue) AS CIFValue,
                    ISNULL(0.00, E.BasicCustomDuty) AS BasicCustomDuty,
                    ISNULL(0.00, E.SocialWelfareSurCharges) AS SocialWelfareSurCharges,
                    ISNULL(0.00, E.AgriInfraDevCess) AS AgriInfraDevCess,
                    ISNULL(0.00, E.AntiDumpingDuty) AS AntiDumpingDuty,
                    ISNULL(0.00, E.SafeguardDuty) AS SafeguardDuty,
                    ISNULL(0.00, E.HealthEducationCess) AS HealthEducationCess,
                    ISNULL(C.GSTPercentage, 0.00) AS GSTPercentage,
                    ISNULL(C.CGST, 0.00) AS CGST,
                    ISNULL(C.SGST, 0.00) AS SGST,
                    ISNULL(C.IGST, E.IGST) AS IGST,
                    ISNULL(C.CGSTPercentage, 0.00) AS CGSTPercentage,
                    ISNULL(C.SGSTPercentage, 0.00) AS SGSTPercentage,
                    ISNULL(C.IGSTPercentage, E.IGSTPercentage) AS IGSTPercentage,
                    ISNULL(C.UOMId, E.UOMId) AS UOMId,
                    ISNULL(C.ItemSno, E.ItemSno) AS ItemSno
                FROM Purchase.PurchaseOrderHeader A

                LEFT JOIN Purchase.PurchaseLocalHeader B 
                    ON A.Id = B.PurchaseOrderId
                LEFT JOIN Purchase.PurchaseLocalDetail C 
                    ON B.Id = C.PurchaseLocalId
                    AND C.ItemId = @ItemId
                    AND C.ItemSno = @PoSlNoLocal

                LEFT JOIN Purchase.PurchaseOrderImportHeader D 
                    ON A.Id = D.PurchaseOrderId
                LEFT JOIN Purchase.PurchaseOrderImportDetail E 
                    ON D.Id = E.PurchaseHeaderId
                    AND E.ItemId = @ItemId
                    AND E.ItemSno = @PoSlNoLocal

                WHERE A.Id = @PoId
                AND A.POMethodId = @PoMethodId
                AND A.POCategoryId = @PoCategoryId;
            ";

            var result = await _dbConnection.QueryAsync<PoValueDetailsDto>(
                sql, 
                new
                {
                    PoId,
                    PoSlNoLocal,
                    PoCategoryId,
                    PoMethodId,
                    ItemId
                }
            );

            return result.ToList();
        }

    }
}