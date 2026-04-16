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
using Contracts.Interfaces.Lookups.Sales;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Validations.SalesManagement;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Contracts.Interfaces.Validations.FinanceManagement;
using Contracts.Interfaces.Lookups.Logistics;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.PartyMaster
{
    public class PartyMasterQueryRepository : IPartyMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        private readonly IIncotermLookup _incotermLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly ICityLookup _cityLookup;
        private readonly IStateLookup _stateLookup;
        private readonly ICountryLookup _countryLookup;
        private readonly IDataAccessFilter _dataAccessFilter;
        private readonly ISalesSegmentLookup _salesSegmentLookup;
        private readonly IPartyMasterSalesValidation _salesValidation;
        private readonly IPartyMasterPurchaseValidation _purchaseValidation;
        private readonly IPartyMasterFinanceValidation _financeValidation;
        private readonly IPartyMasterMaintenanceValidation _maintenanceValidation;
        private readonly IFreightMasterLookup _freightMasterLookup;

        public PartyMasterQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService,
            IIncotermLookup incotermLookup, IPaymentTermLookup paymentTermLookup,
            ICityLookup cityLookup, IStateLookup stateLookup, ICountryLookup countryLookup,
            IDataAccessFilter dataAccessFilter, ISalesSegmentLookup salesSegmentLookup,
            IPartyMasterSalesValidation salesValidation,
            IPartyMasterPurchaseValidation purchaseValidation,
            IPartyMasterFinanceValidation financeValidation,
            IPartyMasterMaintenanceValidation maintenanceValidation,
            IFreightMasterLookup freightMasterLookup)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
            _incotermLookup = incotermLookup;
            _paymentTermLookup = paymentTermLookup;
            _cityLookup = cityLookup;
            _stateLookup = stateLookup;
            _countryLookup = countryLookup;
            _dataAccessFilter = dataAccessFilter;
            _salesSegmentLookup = salesSegmentLookup;
            _salesValidation = salesValidation;
            _purchaseValidation = purchaseValidation;
            _financeValidation = financeValidation;
            _maintenanceValidation = maintenanceValidation;
            _freightMasterLookup = freightMasterLookup;
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
            SELECT pm.*
            FROM Party.PartyMaster pm
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
            SELECT
                td.Id,
                td.PartyId,
                td.TransportModeId,
                tm.Description AS TransportModeName,
                td.VehicleTypeId,
                vt.Description AS VehicleTypeName,
                td.DefaultFreightTypeId,
                dft.Description AS DefaultFreightTypeName,
                td.DefaultFreightRate,
                td.LicenseNo,
                td.LicenseExpiryDate,
                td.VehicleNo,
                td.Status
            FROM Party.TransportDetail td
            LEFT JOIN Party.MiscMaster tm ON td.TransportModeId = tm.Id AND tm.IsDeleted = 0
            LEFT JOIN Party.MiscMaster vt ON td.VehicleTypeId = vt.Id AND vt.IsDeleted = 0
            LEFT JOIN Party.MiscMaster dft ON td.DefaultFreightTypeId = dft.Id AND dft.IsDeleted = 0
            WHERE td.PartyId = @Id;

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
            partyMaster.TransportDetails = (await multi.ReadAsync<PartyMasterDto.TransportDetailDto>()).ToList();

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

            // Populate freight details via cross-module lookup
            if (partyMaster.SalesFreightId.HasValue && partyMaster.SalesFreightId > 0)
            {
                var salesFreight = await _freightMasterLookup.GetByIdAsync(partyMaster.SalesFreightId.Value);
                if (salesFreight != null)
                {
                    partyMaster.SalesFreightModeName = salesFreight.FreightModeName;
                    partyMaster.SalesRateMethodName = salesFreight.RateMethodName;
                    partyMaster.SalesFreightRate = salesFreight.Rate;
                }
            }

            if (partyMaster.PurchaseFreightId.HasValue && partyMaster.PurchaseFreightId > 0)
            {
                var purchaseFreight = await _freightMasterLookup.GetByIdAsync(partyMaster.PurchaseFreightId.Value);
                if (purchaseFreight != null)
                {
                    partyMaster.PurchaseFreightModeName = purchaseFreight.FreightModeName;
                    partyMaster.PurchaseRateMethodName = purchaseFreight.RateMethodName;
                    partyMaster.PurchaseFreightRate = purchaseFreight.Rate;
                }
            }

            return partyMaster;
        }

        public async Task<(List<GetPartyMasterDto>, int)> GetAllPartyMasterAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            // Data access control — marketing officer and agent customer filtering (no bypass in Party)
            var accessCtx = await _dataAccessFilter.GetContextAsync();

            // Build allowed party IDs: mapped customer IDs only (agent's own PartyId is excluded)
            var allowedPartyIds = new HashSet<int>(accessCtx.AllowedCustomerIds);

            if (accessCtx.IsCustomerRestricted && allowedPartyIds.Count == 0)
                return (new List<GetPartyMasterDto>(), 0);

            var customerFilter = accessCtx.IsCustomerRestricted && allowedPartyIds.Count > 0
                ? "AND a.Id IN @AllowedPartyIds"
                : "";

            var searchFilter = string.IsNullOrEmpty(SearchTerm)
                ? ""
                : "WHERE (PartyName LIKE @Search OR PartyCode LIKE @Search OR Party_GroupType LIKE @Search)";

            var query = $@"
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
                    {customerFilter}
                    GROUP BY a.Id, a.PartyCode, a.PartyName, c.Description, a.GSTNumber, a.PAN, a.Website, a.PartyStatus, a.IsActive,a.CreatedDate
                )
                SELECT *,
                    COUNT(*) OVER() AS TotalCount
                FROM PartyMaster_CTE
                {searchFilter}
                ORDER BY CreatedDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            ";

            var dp = new DynamicParameters();
            dp.Add("Search", $"%{SearchTerm}%");
            dp.Add("Offset", (PageNumber - 1) * PageSize);
            dp.Add("PageSize", PageSize);
            if (accessCtx.IsCustomerRestricted && allowedPartyIds.Count > 0)
                dp.Add("AllowedPartyIds", allowedPartyIds.ToList());

            var result = await _dbConnection.QueryAsync<GetPartyMasterDto, int, (GetPartyMasterDto, int)>(
                query,
                (dto, totalCount) => (dto, totalCount),
                dp,
                splitOn: "TotalCount"
            );

            var partyMasters = result.Select(r => r.Item1).ToList();
            int totalCount = result.Any() ? result.First().Item2 : 0;

            // Populate PartyTypes for each party (same as GetByIdPartyMasterAsync)
            if (partyMasters.Count > 0)
            {
                var partyIds = partyMasters.Select(p => p.Id).ToList();

                const string partyTypeSql = @"
                    SELECT A.Id, A.PartyId, A.PartyTypeId, A.PartyGroupId,
                           C.Description AS GlCategory
                    FROM Party.PartyType A
                    INNER JOIN Party.PartyGroup B ON A.PartyGroupId = B.Id
                    INNER JOIN Party.MiscMaster C ON B.GlCategoryId = C.Id
                    WHERE A.PartyId IN @PartyIds";

                var partyTypes = (await _dbConnection.QueryAsync<PartyTypeItemDto>(
                    partyTypeSql, new { PartyIds = partyIds })).ToList();

                var partyTypesByParty = partyTypes.GroupBy(pt => pt.PartyId)
                    .ToDictionary(g => g.Key ?? 0, g => g.ToList());

                foreach (var party in partyMasters)
                {
                    if (partyTypesByParty.TryGetValue(party.Id, out var types))
                        party.PartyTypes = types;
                }
            }

            return (partyMasters, totalCount);
        }

        public async Task<List<GetPartyMasterAutoCompleteDto>> GetPartyMasterAutoComplete(List<int> partyTypeIds, string searchPattern)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;

            // Data access control — marketing officer and agent customer filtering (no bypass in Party)
            var accessCtx = await _dataAccessFilter.GetContextAsync();
            if (accessCtx.IsCustomerRestricted && accessCtx.AllowedCustomerIds.Count == 0)
                return new List<GetPartyMasterAutoCompleteDto>();

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

            // ✅ Apply customer access filter (agent-level or marketing officer — no bypass in Party)
            if (accessCtx.IsCustomerRestricted && accessCtx.AllowedCustomerIds.Count > 0)
            {
                sql += " AND a.Id IN @AllowedCustomerIds";
            }

            // ✅ Group after filtering
            sql += @"
                GROUP BY
                    a.Id, a.PartyCode, a.PartyName, d.Description, a.GSTNumber,
                    pc.EmailID, pc.MobileNo,UC.UnitId
                    ORDER BY a.PartyName ASC";

            var dp = new DynamicParameters();
            dp.Add("PartyTypeIds", partyTypeIds);
            dp.Add("SearchPattern", $"%{searchPattern}%");
            dp.Add("MiscTypeCode", MiscEnumEntity.PartyDocumentImage.Approved);
            dp.Add("UnitId", UnitId);
            if (accessCtx.IsCustomerRestricted && accessCtx.AllowedCustomerIds.Count > 0)
                dp.Add("AllowedCustomerIds", accessCtx.AllowedCustomerIds.ToList());

            var result = (await _dbConnection.QueryAsync<GetPartyMasterAutoCompleteDto>(sql, dp)).ToList();

            if (result.Count > 0)
            {
                var partyIds = result.Select(p => p.Id).ToList();

                // Fetch addresses
                const string addressSql = @"
                    SELECT pa.Id, pa.PartyId, pa.AddressType, pa.AddressLine1, pa.AddressLine2, pa.PostalCode,
                        pa.CityId, pa.StateId, pa.CountryId
                    FROM Party.PartyAddress pa
                    WHERE pa.PartyId IN @PartyIds";

                var flatAddresses = (await _dbConnection.QueryAsync<PartyAddressFlatDto>(addressSql, new { PartyIds = partyIds })).ToList();

                if (flatAddresses.Count > 0)
                {
                    var cityIds = flatAddresses.Where(a => a.CityId.HasValue).Select(a => a.CityId.Value).Distinct();
                    var stateIds = flatAddresses.Where(a => a.StateId.HasValue).Select(a => a.StateId.Value).Distinct();
                    var countryIds = flatAddresses.Where(a => a.CountryId.HasValue).Select(a => a.CountryId.Value).Distinct();

                    var cities = cityIds.Any() ? await _cityLookup.GetByIdsAsync(cityIds) : [];
                    var states = stateIds.Any() ? await _stateLookup.GetByIdsAsync(stateIds) : [];
                    var countries = countryIds.Any() ? await _countryLookup.GetByIdsAsync(countryIds) : [];

                    var cityDict = cities.ToDictionary(c => c.CityId, c => c.CityName);
                    var stateDict = states.ToDictionary(s => s.StateId, s => s.StateName);
                    var countryDict = countries.ToDictionary(c => c.CountryId, c => c.CountryName);

                    var addressByParty = flatAddresses.GroupBy(a => a.PartyId).ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var party in result)
                    {
                        if (addressByParty.TryGetValue(party.Id, out var addresses))
                        {
                            party.PartyAddresses = addresses.Select(a => new PartyAddressAutoCompleteDto
                            {
                                Id = a.Id,
                                AddressType = a.AddressType,
                                AddressLine1 = a.AddressLine1,
                                AddressLine2 = a.AddressLine2,
                                PostalCode = a.PostalCode,
                                CityId = a.CityId,
                                CityName = a.CityId.HasValue && cityDict.TryGetValue(a.CityId.Value, out var cn) ? cn : null,
                                StateId = a.StateId,
                                StateName = a.StateId.HasValue && stateDict.TryGetValue(a.StateId.Value, out var sn) ? sn : null,
                                CountryId = a.CountryId,
                                CountryName = a.CountryId.HasValue && countryDict.TryGetValue(a.CountryId.Value, out var countryName) ? countryName : null,
                            }).ToList();
                        }
                    }
                }

                // Fetch contacts (Gender, PreferredChannel, ContactType from same-module MiscMaster)
                const string contactSql = @"
                    SELECT pc.Id, pc.PartyId, pc.FirstName, pc.LastName, pc.Designation,
                        pc.EmailID, pc.MobileNo, pc.Phone,
                        pc.GenderId, g.Description AS GenderName,
                        pc.PreferredChannelId, ch.Description AS PreferredChannelName,
                        pc.ContactTypeId, ct.Description AS ContactTypeName,
                        pc.ContactBy
                    FROM Party.PartyContact pc
                    LEFT JOIN Party.MiscMaster g  ON pc.GenderId          = g.Id  AND g.IsDeleted  = 0
                    LEFT JOIN Party.MiscMaster ch ON pc.PreferredChannelId = ch.Id AND ch.IsDeleted = 0
                    LEFT JOIN Party.MiscMaster ct ON pc.ContactTypeId      = ct.Id AND ct.IsDeleted = 0
                    WHERE pc.PartyId IN @PartyIds";

                var flatContacts = (await _dbConnection.QueryAsync<PartyContactFlatDto>(contactSql, new { PartyIds = partyIds })).ToList();

                if (flatContacts.Count > 0)
                {
                    var contactByParty = flatContacts.GroupBy(c => c.PartyId).ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var party in result)
                    {
                        if (contactByParty.TryGetValue(party.Id, out var contacts))
                        {
                            party.PartyContacts = contacts.Select(c => new PartyContactAutoCompleteDto
                            {
                                Id = c.Id,
                                FirstName = c.FirstName,
                                LastName = c.LastName,
                                Designation = c.Designation,
                                EmailID = c.EmailID,
                                MobileNo = c.MobileNo,
                                Phone = c.Phone,
                                GenderId = c.GenderId,
                                GenderName = c.GenderName,
                                PreferredChannelId = c.PreferredChannelId,
                                PreferredChannelName = c.PreferredChannelName,
                                ContactTypeId = c.ContactTypeId,
                                ContactTypeName = c.ContactTypeName,
                                ContactBy = c.ContactBy,
                            }).ToList();
                        }
                    }
                }

                // ─── Step 4: Fetch SalesTypes with SalesSegment names ───
                const string salesTypeSql = @"
                    SELECT Id, PartyId, SalesSegmentId
                    FROM Party.SalesType
                    WHERE PartyId IN @PartyIds";

                var flatSalesTypes = (await _dbConnection.QueryAsync<SalesTypeFlatDto>(salesTypeSql, new { PartyIds = partyIds })).ToList();

                if (flatSalesTypes.Count > 0)
                {
                    var allSegments = await _salesSegmentLookup.GetAllSalesSegmentAsync();
                    var segmentDict = allSegments.ToDictionary(s => s.Id, s => s.SegmentName);

                    var salesTypesByParty = flatSalesTypes.GroupBy(st => st.PartyId).ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var party in result)
                    {
                        if (salesTypesByParty.TryGetValue(party.Id, out var salesTypes))
                        {
                            party.SalesTypes = salesTypes.Select(st => new SalesTypeAutoCompleteDto
                            {
                                Id = st.Id,
                                SalesSegmentId = st.SalesSegmentId,
                                SegmentName = st.SalesSegmentId.HasValue && segmentDict.TryGetValue(st.SalesSegmentId.Value, out var name)
                                    ? name : null
                            }).ToList();
                        }
                    }
                }

                // ─── Step 5: Fetch TransportDetails (Status = 1 only) ───
                const string transportSql = @"
                    SELECT td.Id, td.PartyId, td.TransportModeId, tm.Description AS TransportModeName,
                        td.VehicleTypeId, vt.Description AS VehicleTypeName,
                        td.DefaultFreightTypeId, dft.Description AS DefaultFreightTypeName,
                        td.DefaultFreightRate, td.LicenseNo, td.LicenseExpiryDate,
                        td.VehicleNo, td.Status
                    FROM Party.TransportDetail td
                    LEFT JOIN Party.MiscMaster tm ON td.TransportModeId = tm.Id AND tm.IsDeleted = 0
                    LEFT JOIN Party.MiscMaster vt ON td.VehicleTypeId = vt.Id AND vt.IsDeleted = 0
                    LEFT JOIN Party.MiscMaster dft ON td.DefaultFreightTypeId = dft.Id AND dft.IsDeleted = 0
                    WHERE td.PartyId IN @PartyIds AND td.Status = 1";

                var flatTransports = (await _dbConnection.QueryAsync<TransportDetailAutoCompleteDto>(transportSql, new { PartyIds = partyIds })).ToList();

                if (flatTransports.Count > 0)
                {
                    var transportByParty = flatTransports.GroupBy(t => t.PartyId).ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var party in result)
                    {
                        if (transportByParty.TryGetValue(party.Id, out var transports))
                            party.TransportDetails = transports;
                    }
                }

            }

            return result;
        }

        public async Task<List<GetPartyMasterAutoCompleteDto>> GetPartyMasterAutoComplete(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty;

            // Data access control — marketing officer and agent customer filtering (no bypass in Party)
            var accessCtx = await _dataAccessFilter.GetContextAsync();
            if (accessCtx.IsCustomerRestricted && accessCtx.AllowedCustomerIds.Count == 0)
                return new List<GetPartyMasterAutoCompleteDto>();

            var sql = @"
             SELECT Id, PartyCode, PartyName
            FROM Party.PartyMaster
            WHERE IsDeleted = 0 AND IsActive = 1
            AND (PartyName LIKE @SearchPattern OR PartyCode LIKE @SearchPattern)";

            if (accessCtx.IsCustomerRestricted && accessCtx.AllowedCustomerIds.Count > 0)
                sql += " AND Id IN @AllowedCustomerIds";

            var dp = new DynamicParameters();
            dp.Add("SearchPattern", $"%{searchPattern}%");
            if (accessCtx.IsCustomerRestricted && accessCtx.AllowedCustomerIds.Count > 0)
                dp.Add("AllowedCustomerIds", accessCtx.AllowedCustomerIds.ToList());

            var partyMasters = await _dbConnection.QueryAsync<GetPartyMasterAutoCompleteDto>(sql, dp);
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

        public async Task<bool> TransportDetailDuplicateExistsAsync(int? defaultFreightTypeId, int? vehicleTypeId, string vehicleNo, int? excludeId = null)
        {
            var sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Party.TransportDetail
                    WHERE DefaultFreightTypeId = @DefaultFreightTypeId
                      AND VehicleTypeId = @VehicleTypeId
                      AND VehicleNo = @VehicleNo
                      AND Status = 1";

            if (excludeId.HasValue)
                sql += " AND Id != @ExcludeId";

            sql += ") THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new
            {
                DefaultFreightTypeId = defaultFreightTypeId,
                VehicleTypeId = vehicleTypeId,
                VehicleNo = vehicleNo,
                ExcludeId = excludeId
            });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string query = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Party.PartyMaster
                    WHERE Id = @id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;";

            return !await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Same-module children (no IsDeleted column — plain EXISTS)
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM Party.PartyContact WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyAddress WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyBank WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyDocument WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyType WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.SalesType WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyUnitCompanyMapping WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.TransportDetail WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.AgentConfig WHERE PartyId = @id)
                THEN 1 ELSE 0 END;";

            var sameModule = await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
            if (sameModule) return true;

            // Cross-module children via validation interfaces
            if (await _salesValidation.HasLinkedPartyMasterAsync(id)) return true;
            if (await _purchaseValidation.HasLinkedPartyMasterAsync(id)) return true;
            if (await _financeValidation.HasLinkedPartyMasterAsync(id)) return true;
            if (await _maintenanceValidation.HasLinkedPartyMasterAsync(id)) return true;

            return false;
        }

        public async Task<bool> IsPartyMasterLinkedAsync(int id)
        {
            // Same-module children (no IsDeleted/IsActive column — plain EXISTS)
            const string query = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM Party.PartyContact WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyAddress WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyBank WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyDocument WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyType WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.SalesType WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.PartyUnitCompanyMapping WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.TransportDetail WHERE PartyId = @id)
                    OR EXISTS (SELECT 1 FROM Party.AgentConfig WHERE PartyId = @id)
                THEN 1 ELSE 0 END;";

            var sameModule = await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
            if (sameModule) return true;

            // Cross-module children via validation interfaces
            if (await _salesValidation.HasActivePartyMasterAsync(id)) return true;
            if (await _purchaseValidation.HasActivePartyMasterAsync(id)) return true;
            if (await _financeValidation.HasActivePartyMasterAsync(id)) return true;
            if (await _maintenanceValidation.HasActivePartyMasterAsync(id)) return true;

            return false;
        }
    }
}