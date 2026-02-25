#nullable disable
using System.Data;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo;
using PurchaseManagement.Domain.Common;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.GRN.GateEntry
{
    public class GateEntryQueryRepository : IGateEntryQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public GateEntryQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
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
            var parameters = new { MiscTypeCode = MiscEnumEntity.GateEntryImage};
            var result = await _dbConnection.QueryAsync<string>(query, parameters);
            return result.FirstOrDefault();
        }

        public async Task<List<GetGateEntriesApprovedPoDto>> GetGateEntriesApprovedPoDto(int partyId)
        {
            var UnitId = _ipAddressService.GetUnitId();
            var query = @"
                    WITH GrnSummary AS
                    (
                        SELECT 
                            gd.PoId,
                            SUM(gd.DcQuantity) AS TotalGrnQty
                        FROM Purchase.GrnDetail gd
                        INNER JOIN Purchase.GrnHeader gh ON gd.GrnId = gh.Id
                        GROUP BY gd.PoId
                    ),

                    -- ============================
                    -- LOCAL PO BLOCK
                    -- ============================
                    LocalPO AS
                    (
                        SELECT 
                            poh.Id AS PoId,
                            poh.PoDate,
                            poh.PONumber,
                            poh.UnitId,
                            poh.CreatedDate,
                            poh.CreatedByName,
                            poh.VendorId,
                            poh.POCategoryId,
                            poh.POMethodId,
                            SUM(pod.Quantity) AS TotalOrderQty
                        FROM Purchase.PurchaseOrderHeader poh
                        INNER JOIN Purchase.PurchaseLocalHeader plh ON poh.Id = plh.PurchaseOrderId
                        INNER JOIN Purchase.PurchaseLocalDetail pod ON plh.Id = pod.PurchaseLocalId
                        INNER JOIN Purchase.MiscMaster MM ON poh.StatusId = MM.Id
                        WHERE MM.Description = @MiscTypeCode and poh.VendorId = @PartyId and poh.UnitId = @UnitId and poh.IsDeleted = 0
                        GROUP BY 
                            poh.Id, poh.PoDate,poh.PONumber, poh.UnitId, poh.CreatedDate, poh.CreatedByName,
                            poh.VendorId, poh.POCategoryId, poh.POMethodId
                            
                    ),

                    -- ============================
                    -- IMPORT PO BLOCK
                    -- ============================
                    ImportPO AS
                    (
                        SELECT 
                            poh.Id AS PoId,
                            poh.PoDate,
                            poh.PONumber,
                            poh.UnitId,
                            poh.CreatedDate,
                            poh.CreatedByName,
                            poh.VendorId,
                            poh.POCategoryId,
                            poh.POMethodId,
                            SUM(poid.Quantity) AS TotalOrderQty
                        FROM Purchase.PurchaseOrderHeader poh
                        INNER JOIN Purchase.PurchaseOrderImportHeader poih ON poh.Id = poih.PurchaseOrderId
                        INNER JOIN Purchase.PurchaseOrderImportDetail poid ON poih.Id = poid.PurchaseHeaderId
                        INNER JOIN Purchase.MiscMaster MM ON poh.StatusId = MM.Id
                        WHERE MM.Description =  @MiscTypeCode and poh.VendorId = @PartyId and poh.UnitId = @UnitId and poh.IsDeleted = 0
                        GROUP BY 
                            poh.Id, poh.PoDate,poh.PONumber,poh.UnitId, poh.CreatedDate, poh.CreatedByName,
                            poh.VendorId, poh.POCategoryId, poh.POMethodId
                            
                    ),

                    -- ============================
                    -- COMBINE LOCAL + IMPORT PO
                    -- ============================
                    AllPO AS
                    (
                        SELECT * FROM LocalPO
                        UNION ALL
                        SELECT * FROM ImportPO
                    )

                    -- ============================
                    -- FINAL SELECT
                    -- ============================
                    SELECT 
                        ap.PoId,
                        ap.PoDate,
                        ap.PONumber,
                        ap.UnitId,
                        ap.CreatedDate,
                        ap.CreatedByName,
                        ap.VendorId,
                        ap.POCategoryId,
                        ap.POMethodId,
                        ap.TotalOrderQty,
                        ISNULL(gs.TotalGrnQty, 0) AS TotalGrnQty,
                        ap.TotalOrderQty - ISNULL(gs.TotalGrnQty, 0) AS PendingQty
                    FROM AllPO ap
                    LEFT JOIN GrnSummary gs ON gs.PoId = ap.PoId
                    WHERE ap.UnitId = @UnitId and ap.VendorId = @PartyId
                    AND (ap.TotalOrderQty - ISNULL(gs.TotalGrnQty, 0)) > 0
                    ORDER BY ap.PoId;
                    ";

                            var result = await _dbConnection.QueryAsync<GetGateEntriesApprovedPoDto>(
                                query,
                                new { PartyId = partyId, UnitId = UnitId, MiscTypeCode = MiscEnumEntity.Approved }
                            );

            return result.AsList();

        }
    }
}
