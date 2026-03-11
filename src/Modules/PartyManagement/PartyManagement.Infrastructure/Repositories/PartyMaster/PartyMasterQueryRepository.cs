#nullable disable
using System.Data;
using Contracts.Interfaces;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete;
using PartyManagement.Application.PartyMaster.Queries.GetPartyGroupLoad;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterPending;
using PartyManagement.Domain.Common;
using Contracts.Interfaces.Lookups.Purchase;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.PartyMaster
{
    public class PartyMasterQueryRepository : IPartyMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        private readonly IIncotermLookup _incotermLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;

        public PartyMasterQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService,
            IIncotermLookup incotermLookup, IPaymentTermLookup paymentTermLookup)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
            _incotermLookup = incotermLookup;
            _paymentTermLookup = paymentTermLookup;
        }
        public async Task<List<PartyGroupLoadDto>> GetPartyGroupsAsync(List<int> groupTypeIds)
        {
            var query = @"
                SELECT 
                    pg.Id GroupId, 
                    ISNULL(ppg.PartyGroupName, '') + '-' + pg.PartyGroupName AS PartyGroupName,
                    mm.Id as PartyTypeId,
	                mm.description as PartyTypeName
                FROM Party.PartyGroup pg
                LEFT JOIN Party.PartyGroup ppg 
                    ON pg.ParentPartyGroupId = ppg.Id
                INNER JOIN Party.MiscMaster mm
                    ON pg.GroupTypeId = mm.Id
                WHERE pg.IsDeleted = 0 
                  AND pg.IsGroup = 0 
                  AND pg.IsActive = 1
                  AND mm.Id IN @GroupTypeIds";

            var result = await _dbConnection.QueryAsync<PartyGroupLoadDto>(query, new { GroupTypeIds = groupTypeIds });
            return result.ToList();
        }

        public async Task<string> GetDocumentDirectoryAsync()
        {
            const string query = @"
            SELECT Description            
            FROM Party.MiscTypeMaster             
            WHERE (MiscTypeCode = @MiscTypeCode) 
            AND  IsDeleted=0 and IsActive=1
            ORDER BY ID DESC";
            var parameters = new { MiscTypeCode = MiscEnumEntity.PartyDocumentImage.MiscCode };
            var result = await _dbConnection.QueryAsync<string>(query, parameters);
            return result.FirstOrDefault();
        }
        public async Task<string> GetBaseDirectoryAsync()
        {
            var result = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                "dbo.Party_GetBaseDirectory",
                commandType: CommandType.StoredProcedure);
            return result ?? string.Empty; // return an empty string if result is null
        }

        public async Task<PartyMasterDto> GetByIdPartyMasterAsync(int id)
        {
            var sql = @"
            SELECT pm.*,
                   tm.Description AS TransportModeName,
                   vt.Description AS VehicleTypeName,
                   dft.Description AS DefaultFreightTypeName
            FROM Party.PartyMaster pm
            LEFT JOIN Party.MiscMaster tm ON pm.TransportModeId = tm.Id AND tm.IsDeleted = 0
            LEFT JOIN Party.MiscMaster vt ON pm.VehicleTypeId = vt.Id AND vt.IsDeleted = 0
            LEFT JOIN Party.MiscMaster dft ON pm.DefaultFreightTypeId = dft.Id AND dft.IsDeleted = 0
            WHERE pm.Id = @Id;
            select A.Id,A.PartyId,A.PartyTypeId,A.PartyGroupId,C.description as GlCategory from Party.PartyType A INNER JOIN Party.PartyGroup B ON A.PartyGroupId=B.Id INNER JOIN Party.MiscMaster C ON B.GlCategoryId=C.Id where a.PartyId=@Id;
            SELECT * FROM Party.PartyContact WHERE PartyId = @Id;
            SELECT * FROM Party.PartyAddress WHERE PartyId = @Id;
            SELECT * FROM Party.PartyBank WHERE PartyId = @Id;
            SELECT * FROM Party.PartyDocument WHERE PartyId = @Id;
            SELECT * FROM Party.PartyUnitCompanyMapping WHERE PartyId = @Id;
            SELECT
                st.Id,
                st.PartyId,
                st.SalesSegmentId,
                st.OrderTypeId,
                st.IncotermId,
                st.PaymentTermsId,
                st.ShippingConditionId,
                sc.Description AS ShippingConditionName,
                st.AccountAssignmentId,
                aa.Description AS AccountAssignmentName,
                st.Active
            FROM Party.SalesType st
            LEFT JOIN Party.MiscMaster sc ON st.ShippingConditionId = sc.Id AND sc.IsDeleted = 0
            LEFT JOIN Party.MiscMaster aa ON st.AccountAssignmentId = aa.Id AND aa.IsDeleted = 0
            WHERE st.PartyId = @Id;
            SELECT
                ac.Id,
                ac.PartyId,
                ac.SettlementCycleId,
                mm.Description AS SettlementCycleName,
                ac.TdsApplicable,
                ac.TdsCode,
                ac.DefaultCommissionGl,
                ac.AgreementStartDate,
                ac.AgreementEndDate,
                ac.AgentPayableControlGl,
                ac.TargetAmount,
                ac.TargetPeriod,
                ac.Status
            FROM Party.AgentConfig ac
            LEFT JOIN Party.MiscMaster mm ON ac.SettlementCycleId = mm.Id AND mm.IsDeleted = 0
            WHERE ac.PartyId = @Id;

        ";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id });
            var partyMaster = await multi.ReadFirstOrDefaultAsync<PartyMasterDto>();

            if (partyMaster is null)
                return null; // Let handler handle NotFound

            partyMaster.PartyTypes = (await multi.ReadAsync<PartyMasterDto.PartyTypeDto>()).ToList();
            partyMaster.PartyContacts = (await multi.ReadAsync<PartyMasterDto.PartyContactDto>()).ToList();
            partyMaster.PartyAddresses = (await multi.ReadAsync<PartyMasterDto.PartyAddressDto>()).ToList();
            partyMaster.PartyBanks = (await multi.ReadAsync<PartyMasterDto.PartyBankDto>()).ToList();
            partyMaster.PartyDocuments = (await multi.ReadAsync<PartyMasterDto.PartyDocumentDto>()).ToList();
            partyMaster.PartyUnitCompanyMappings = (await multi.ReadAsync<PartyMasterDto.PartyUnitCompanyMappingDto>()).ToList();
            partyMaster.SalesTypes = (await multi.ReadAsync<PartyMasterDto.SalesTypeDto>()).ToList();
            partyMaster.AgentConfigs = (await multi.ReadAsync<PartyMasterDto.AgentConfigDto>()).ToList();

            // Populate cross-module lookup names for SalesTypes
            if (partyMaster.SalesTypes?.Any() == true)
            {
                var incotermIds = partyMaster.SalesTypes.Where(s => s.IncotermId.HasValue).Select(s => s.IncotermId!.Value).Distinct().ToList();
                var paymentTermIds = partyMaster.SalesTypes.Where(s => s.PaymentTermsId.HasValue).Select(s => s.PaymentTermsId!.Value).Distinct().ToList();

                if (incotermIds.Any())
                {
                    var incoterms = await _incotermLookup.GetAllIncotermAsync();
                    var incotermDict = incoterms.ToDictionary(i => i.Id, i => i.Description);
                    foreach (var st in partyMaster.SalesTypes.Where(s => s.IncotermId.HasValue))
                    {
                        incotermDict.TryGetValue(st.IncotermId!.Value, out var name);
                        st.IncotermName = name;
                    }
                }

                if (paymentTermIds.Any())
                {
                    var paymentTerms = await _paymentTermLookup.GetAllPaymentTermAsync();
                    var ptDict = paymentTerms.ToDictionary(p => p.Id, p => p.Description);
                    foreach (var st in partyMaster.SalesTypes.Where(s => s.PaymentTermsId.HasValue))
                    {
                        ptDict.TryGetValue(st.PaymentTermsId!.Value, out var name);
                        st.PaymentTermsName = name;
                    }
                }
            }

            return partyMaster;
        }

        public async Task<(List<GetPartyMasterDto>, int)> GetAllPartyMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var query = $$"""
                ;WITH PartyMaster_CTE AS (
                    SELECT 
                        a.Id,
                        a.PartyCode,
                        a.PartyName,
                        c.Description AS RegistrationType,
                        a.GSTNumber,
                        a.PAN,
                        a.Website,
                        STRING_AGG(CONCAT(e.PartyGroupName, '-', d.Description), ',') AS Party_GroupType,
                        a.IsActive,
                        a.PartyStatus,
                        a.CreatedDate
                    FROM Party.PartyMaster a
                    INNER JOIN Party.PartyType b ON a.Id = b.PartyId
                    INNER JOIN Party.MiscMaster c ON a.RegistrationTypeId = c.Id
                    INNER JOIN Party.MiscMaster d ON b.PartyTypeId = d.Id
                    INNER JOIN Party.PartyGroup e ON e.Id = b.PartyGroupId
                    WHERE a.IsDeleted = 0
                    GROUP BY a.Id, a.PartyCode, a.PartyName, c.Description, a.GSTNumber, a.PAN, a.Website, a.PartyStatus, a.IsActive,a.CreatedDate
                )
                SELECT *,
                    COUNT(*) OVER() AS TotalCount
                FROM PartyMaster_CTE
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "WHERE (PartyName LIKE @Search OR PartyCode LIKE @Search OR Party_GroupType LIKE @Search)")}}
                ORDER BY CreatedDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var result = await _dbConnection.QueryAsync<GetPartyMasterDto, int, (GetPartyMasterDto, int)>(
                query,
                (dto, totalCount) => (dto, totalCount),
                parameters,
                splitOn: "TotalCount"
            );

            var partyMasters = result.Select(r => r.Item1).ToList();
            int totalCount = result.Any() ? result.First().Item2 : 0;

            return (partyMasters, totalCount);
        }

        public async Task<List<GetPartyMasterAutoCompleteDto>> GetPartyMasterAutoComplete(List<int> partyTypeIds, string searchPattern)
        {
             var UnitId = _ipAddressService.GetUnitId() ?? 0;
            var sql = @"
                SELECT 
                    a.Id,
                    a.PartyCode,
                    a.PartyName,
                    d.Description AS RegistrationType,
                    a.GSTNumber,
                    CASE
                        WHEN d.Description IN ('Company', 'Individual', 'Partnership') THEN 'Y'
                        WHEN d.Description = 'Un-Registered' THEN 'N'
                        ELSE 'N'
                    END AS GSTFlag,
                    pc.EmailID AS PrimaryEmail,
                    pc.MobileNo AS PrimaryMobile
                FROM Party.PartyMaster a
                INNER JOIN Party.PartyType b 
                    ON a.Id = b.PartyId
                INNER JOIN Party.PartyUnitCompanyMapping UC
				    ON a.Id=UC.PartyId
                INNER JOIN Party.MiscMaster c 
                    ON b.PartyTypeId = c.Id
                INNER JOIN Party.MiscMaster d
                    ON a.RegistrationTypeId = d.Id
                 INNER JOIN Party.MiscMaster MM 
                 ON a.StatusId=MM.Id
                LEFT JOIN Party.PartyContact pc
                    ON a.Id = pc.PartyId
                AND pc.ContactBy = 'Primary'
                WHERE a.IsDeleted = 0
                AND MM.description =  @MiscTypeCode
                AND a.IsActive = 1
                AND UC.UnitId=@UnitId
                AND (a.PartyName LIKE @SearchPattern OR a.PartyCode LIKE @SearchPattern)
                
            ";

            // ✅ Apply optional PartyTypeId filter dynamically
            if (partyTypeIds != null && partyTypeIds.Any())
            {
                sql += " AND b.PartyTypeId IN @PartyTypeIds";
            }

            // ✅ Group after filtering
            sql += @"
                GROUP BY 
                    a.Id, a.PartyCode, a.PartyName, d.Description, a.GSTNumber,
                    pc.EmailID, pc.MobileNo,UC.UnitId
                    ORDER BY a.PartyName ASC";

            var parameters = new
            {
                PartyTypeIds = partyTypeIds,
                SearchPattern = $"%{searchPattern}%",
                MiscTypeCode = MiscEnumEntity.PartyDocumentImage.Approved,
                UnitId=UnitId

            };

            var result = await _dbConnection.QueryAsync<GetPartyMasterAutoCompleteDto>(sql, parameters);

            return result.ToList();
        }

        public async Task<List<GetPartyMasterAutoCompleteDto>> GetPartyMasterAutoComplete(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
             SELECT Id, PartyCode,PartyName 
            FROM Party.PartyMaster 
            WHERE IsDeleted = 0  and IsActive=1
            AND PartyName LIKE @SearchPattern OR PartyCode LIKE @SearchPattern";
            var parameters = new
            {
                SearchPattern = $"%{searchPattern}%"

            };

            var partyMasters = await _dbConnection.QueryAsync<GetPartyMasterAutoCompleteDto>(query, parameters);
            return partyMasters.ToList();
        }

        public async Task<(List<PartyMasterPendingDto>, int)> GetAllPartyMasterPendingAsync(string SearchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;

            var query = $$"""
                ;WITH PartyMaster_CTE AS (
                    SELECT 
                        a.Id,
                        a.PartyCode,
                        a.PartyName,
                        c.Description AS RegistrationType,
                        a.GSTNumber,
                        a.UnitId,
                        a.PAN,
                        a.Website,
                        STRING_AGG(CONCAT(e.PartyGroupName, '-', d.Description), ',') AS Party_GroupType,
                        a.IsActive,
                        a.PartyStatus,
                        a.CreatedBy,
                        a.CreatedDate,
                        a.CreatedByName,
                        a.IsPortalAccessEnabled,
                        a.IsUpdate
                    FROM Party.PartyMaster a
                    INNER JOIN Party.PartyType b ON a.Id = b.PartyId
                    INNER JOIN Party.MiscMaster c ON a.RegistrationTypeId = c.Id
                    INNER JOIN Party.MiscMaster d ON b.PartyTypeId = d.Id
                    INNER JOIN Party.PartyGroup e ON e.Id = b.PartyGroupId
                    WHERE a.IsDeleted = 0 AND a.PartyStatus = @Pending AND a.UnitId = @UnitId
                    GROUP BY a.Id, a.PartyCode, a.PartyName, c.Description, a.GSTNumber, a.PAN, a.Website,
                            a.PartyStatus, a.IsActive, a.CreatedBy, a.CreatedDate, a.CreatedByName, a.UnitId,a.IsPortalAccessEnabled,a.IsUpdate
                )
                SELECT *,
                    ISNULL(COUNT(*) OVER(), 0) AS TotalCount
                FROM PartyMaster_CTE
                {{(string.IsNullOrEmpty(SearchTerm) ? "" :
                "WHERE (Id LIKE @Search OR PartyCode LIKE @Search OR PartyName LIKE @Search)")}}
                ORDER BY PartyCode ASC;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                UnitId,
                Pending = MiscEnumEntity.PartyDocumentImage.Pending
            };

            var result = await _dbConnection.QueryAsync<PartyMasterPendingDto, int, (PartyMasterPendingDto, int)>(
                query,
                (dto, totalCount) => (dto, totalCount),
                parameters,
                splitOn: "TotalCount"
            );

            var partyMasters = result.Select(r => r.Item1).ToList();
            int totalCount = result.Any() ? result.First().Item2 : 0;

            return (partyMasters, totalCount);
        }

        public async Task<(IReadOnlyList<int> CompanyIds, IReadOnlyList<int> UnitIds)> GetCompanyUnitMapAsync(int partyId)
        {
            const string sql = @"
                SELECT DISTINCT CompanyId, UnitId
                FROM Party.PartyUnitCompanyMapping WITH (NOLOCK)
                WHERE PartyId = @PartyId;
            ";

            var rows = await _dbConnection.QueryAsync<(int CompanyId, int UnitId)>(sql, new { PartyId = partyId });

            var companyIds = rows.Select(r => r.CompanyId).Distinct().ToList();
            var unitIds = rows.Select(r => r.UnitId).Distinct().ToList();

            return (companyIds, unitIds);
        }
        
        public async Task<IReadOnlyList<string>> GetPartyTypeCodesAsync(int partyId)
            {
                const string sql = @"
                    SELECT DISTINCT mm.Code
                    FROM Party.PartyType pt
                    JOIN Party.MiscMaster mm ON mm.Id = pt.PartyTypeId
                    WHERE pt.PartyId = @PartyId AND mm.IsActive = 1 AND mm.IsDeleted = 0;
                ";

                var rows = await _dbConnection.QueryAsync<string>(sql, new { PartyId = partyId });

                // Filter out nulls, normalize casing, and return as IReadOnlyList<string>
                var codes = rows
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s!.Trim().ToUpperInvariant())
                    .Distinct()
                    .ToList();

                return codes;
            }

        public async Task<RegistrationDto> GetRegistrationDetails(int RegistrationTypeId)
        {
            var query = @"
            SELECT 
                Id AS RegistrationTypeId,
                Description
            FROM Party.MiscMaster
            WHERE Id = @RegistrationTypeId
              AND IsActive = 1
              AND IsDeleted = 0";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<RegistrationDto>(
            query, 
            new { RegistrationTypeId = RegistrationTypeId }
        );
        return result;
        }

       public async Task<Dictionary<string, string>> GetDocumentDirectoryPath()
    {
        const string sql = @"
            SELECT MiscTypeCode,Description
            FROM [Party].[MiscTypeMaster]
            WHERE MiscTypeCode IN @MiscTypeCodes;";

        var miscCodes = new[]
        {
            MiscEnumEntity.PartyDocumentImage.MiscCode,
            MiscEnumEntity.PartyDocumentImage.GETPARTYIMAGE
        };

        var result = await _dbConnection.QueryAsync<(string MiscTypeCode, string Description)>(
            sql,
            new { MiscTypeCodes = miscCodes }
        );

        // Convert to dictionary: key = MiscTypeCode, value = Description
        return result.ToDictionary(x => x.MiscTypeCode, x => x.Description);
    }
    }
}