using System.Data;
using System.Text;
using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePOPending;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetAllSES;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetApprovedPOList;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServicePOLines;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServiceScheduleByPoAndServiceId;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetSESListToApprove;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.ServiceEntrySheetGetById;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllSES;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Inventory;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder.ServicePO
{
    public class ServicePurchaseOrderQueryRepository : IServicePurchaseOrderQueryRepository
    {
        private readonly IDbConnection _conn;
        private readonly IIPAddressService _ip;

        private readonly ICurrencyLookup _currencyLookup;

        private readonly IPartyLookup _partyLookup;
        private readonly ApplicationDbContext _applicationDb;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        private readonly IUOMLookup _uOMLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IMapper _mapper;


        public ServicePurchaseOrderQueryRepository(IDbConnection conn, IIPAddressService ip, ApplicationDbContext db, ICurrencyLookup currencyLookup, IPartyLookup partyLookup,
            IMiscMasterQueryRepository miscMasterQueryRepository, IUOMLookup uOMLookup, IUnitLookup unitLookup, IMapper mapper)
        {

            _conn = conn;
            _ip = ip;
            _currencyLookup = currencyLookup;
            _partyLookup = partyLookup;
            _applicationDb = db;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _uOMLookup = uOMLookup;
            _unitLookup = unitLookup;
            _mapper = mapper;
        }


        public async Task<PurchaseOrderServiceDetailDto?> GetServicePOByIdAsync(int id, CancellationToken ct)
        {

            var basePath = await _applicationDb.MiscTypeMaster
           .Where(t => t.MiscTypeCode == MiscEnumEntity.ImagePath && t.IsDeleted == 0)
           .Select(t => t.Description)
           .FirstOrDefaultAsync(ct);

            var folder = await _applicationDb.MiscTypeMaster
                .Where(t => t.MiscTypeCode == MiscEnumEntity.POImage && t.IsDeleted == 0)
                .Select(t => t.Description)
                .FirstOrDefaultAsync(ct);

            string prefix = string.Empty;
            if (!string.IsNullOrWhiteSpace(basePath) && !string.IsNullOrWhiteSpace(folder))
            {
                var b = basePath.TrimEnd('/', '\\');
                var f = folder.Trim('/', '\\');
                prefix = $"{b}/{f}/";
            }

            const string sql = @"
            -- 1) header       

                        SELECT TOP 1
                h.[Id], h.[UnitId], h.[PONumber], h.[PODate],
                h.[POCategoryId], mCat.[Code]   AS [POCategory],
                h.[POMethodId],  mMethod.[description] AS [POMethod],
                h.[StatusId],    mStatus.[Code] AS [Status],
                h.[CurrencyId], h.[VendorId],
                h.[ItemTotal], h.[DiscountTotal], h.[PandFTotal], h.[MiscCharges],
                h.[GSTTotal], h.[CGSTTotal], h.[SGSTTotal], h.[IGSTTotal],
                h.[FreightTotal], h.[InsuranceTotal], h.[TDSTotal], h.[AdvanceAmount], h.[PurchaseValue],
                h.[IsActive], h.[IsDeleted], h.[CreatedBy], h.[CreatedDate], h.[CreatedByName], h.[CreatedIP],
                h.[ModifiedBy], h.[ModifiedDate], h.[ModifiedByName], h.[ModifiedIP],
                h.[AmendmentReason], h.[OldPOId], h.[RevisionNo],
                  -- 🔹 New: Edit flag based on status
                CASE 
                    WHEN mStatus.[Code] = @Approved THEN 1
                    WHEN mStatus.[Code] = @Pending  THEN 0
                    ELSE 0
                END AS Edit,

                -- 🔹 Optional: EditReason placeholder
                CAST(NULL AS NVARCHAR(250)) AS EditReason

            FROM Purchase.[PurchaseOrderHeader] h
            LEFT JOIN Purchase.[MiscMaster] mCat    ON mCat.[Id] = h.[POCategoryId]
            LEFT JOIN Purchase.[MiscMaster] mMethod ON mMethod.[Id] = h.[POMethodId]
            LEFT JOIN Purchase.[MiscMaster] mStatus ON mStatus.[Id] = h.[StatusId]
            WHERE h.[Id] =  @Id AND h.[IsDeleted] = 0;

            -- 2) verify it's a Service PO (at least one row in ServiceHeader, not deleted)
            SELECT TOP 1 1 AS IsService
            FROM Purchase.PurchaseOrderServiceHeader sh
            WHERE sh.PurchaseOrderId = @Id AND sh.IsDeleted = 0;

            -- 3) service headers

                SELECT
                sh.[Id],
                sh.[PurchaseOrderId],    sh.[ServiceCategoryId],    cat.[Code] AS [ServiceCategory],    sh.[ContractTypeId],    con.[Code] AS [ContractType],
                sh.[FrequencyId],    fre.[Code] AS [Frequency],    sh.[ValidityFrom],    sh.[ValidityTo],    sh.[TotalOccurrences],    sh.[OverallLimit],
                sh.[TermDescription],    sh.[POImage],    sh.[BillingAddress],    sh.[DeliveryAddress],    sh.[CostCenterId],    
                sh.[FreightCharges],    sh.[ModeOfDispatchId],    modis.[Code]   AS [ModeOfDispatch],    sh.[TermsId],
                trm.[Code]     AS [Terms]  FROM Purchase.[PurchaseOrderServiceHeader] AS sh
            LEFT JOIN Purchase.[MiscMaster] AS cat   ON cat.[Id] = sh.[ServiceCategoryId]
            LEFT JOIN Purchase.[MiscMaster] AS con   ON con.[Id] = sh.[ContractTypeId]
            LEFT JOIN Purchase.[MiscMaster] AS fre   ON fre.[Id] = sh.[FrequencyId]
            LEFT JOIN Purchase.[MiscMaster] AS modis ON modis.[Id]= sh.[ModeOfDispatchId]
            LEFT JOIN Purchase.[MiscMaster] AS trm   ON trm.[Id] = sh.[TermsId]
            WHERE sh.[PurchaseOrderId] = @Id AND sh.[IsDeleted] = 0;
          

            -- 4) service lines (ordered)
           SELECT
            l.[Id],
            l.[PurchaseOrderId],
            l.[ServicePoHeaderId],
            l.[LineNo],              
            l.[RequestId],
            req.Code  AS [Request],
            l.[ServiceId],
            l.[ServiceCode],
            l.[ServiceDescription],
            l.[UOMId],
            l.[PlannedQuantity],
            l.[PlannedRate],
            l.[DiscountId],            
            dis.Code  AS [DiscountCode],
            l.[Discount],
            l.[ItemCost],
            l.[OtherCost],
            l.[OtherCharges],
            l.[GstPercent],
            l.[Remarks],
            l.[IsActive],
            l.[IsDeleted],
            l.[CreatedBy],
            l.[CreatedDate],
            l.[CreatedByName],
            l.[CreatedIP],
            l.[ModifiedBy],
            l.[ModifiedDate],
            l.[ModifiedByName],
            l.[ModifiedIP]
        FROM Purchase.[PurchaseOrderServiceLine] l
        LEFT JOIN Purchase.[MiscMaster] req ON  req.[Id]=l.[RequestId]
        LEFT JOIN Purchase.[MiscMaster] dis ON  dis.[Id]=l.[DiscountId]
        WHERE l.[PurchaseOrderId] = @Id AND l.[IsDeleted] = 0
        ORDER BY l.[LineNo];

            -- 5) schedules (ordered)
            SELECT
                s.Id, s.PurchaseOrderId, s.ServicePoHeaderId, s.ServiceItemId, s.ScheduleNo,
                s.OccurrencePeriod, s.OccurrenceDescription, s.ActivityTypeId  ,act.Code AS ActivityType, s.PlannedDurationHrs,
                s.DueDate, s.ServiceStartDate, s.ServiceEndDate, s.PlannedQuantity, s.PlannedRate, s.PlannedValue,
                s.AutoGenerateSES, s.Remarks,
                s.IsActive, s.IsDeleted, s.CreatedBy, s.CreatedDate, s.CreatedByName, s.CreatedIP,
                s.ModifiedBy, s.ModifiedDate, s.ModifiedByName, s.ModifiedIP
            FROM Purchase.PurchaseOrderServiceSchedule s
            LEFT JOIN Purchase.[MiscMaster] act ON act.[Id] = s.[ActivityTypeId]
            WHERE s.PurchaseOrderId = @Id AND s.IsDeleted = 0
            ORDER BY s.ScheduleNo;

            -- 6) payment terms
                SELECT
            t.[Id], t.[PurchaseOrderId], t.[PaymentTermId], pt.[Code] AS [PaymentTerm],
            t.[AdvancePercent], t.[CreditDays],
            t.[PaymentModelId], pm.[Code] AS [PaymentModel],
            t.[InsuranceId],   ins.[Code] AS [Insurance],
            t.[InsurancePercent], t.[InsuranceAmount], t.[AdvanceAmount],
            t.[BalancePercent], t.[BalanceAmount]
            FROM Purchase.[PurchasePaymentTerm] t
            LEFT JOIN Purchase.[MiscMaster] pt  ON pt.[Id]  = t.[PaymentTermId]
            LEFT JOIN Purchase.[MiscMaster] pm  ON pm.[Id]  = t.[PaymentModelId]
            LEFT JOIN Purchase.[MiscMaster] ins ON ins.[Id] = t.[InsuranceId]
            WHERE t.[PurchaseOrderId] = @Id AND t.[IsDeleted] = 0;

                    SELECT 
            d.DocumentId,
            mm.Code          AS DocumentName,
            d.FileName,
            d.UploadedDate,        
            CONCAT(            
                CASE 
                    WHEN RIGHT(ISNULL(mtm.Description, ''), 1) = '/' 
                        THEN ISNULL(mtm.Description, '')
                    ELSE ISNULL(mtm.Description, '') + '/'
                END,            
                CASE 
                    WHEN LEFT(ISNULL(mt.Description, ''), 1) = '/' 
                        THEN SUBSTRING(ISNULL(mt.Description, ''), 2, LEN(ISNULL(mt.Description, '')))
                    ELSE ISNULL(mt.Description, '')
                END,
                '/',            
                REPLACE(d.FileName, '\', '/')
            ) AS UploadedPath
        FROM Purchase.PurchaseDocuments AS d WITH (NOLOCK)
        JOIN Purchase.MiscMaster      AS mm ON mm.Id = d.DocumentId
        LEFT JOIN Purchase.MiscTypeMaster mtm ON mtm.MiscTypeCode = 'ImagePath'
        LEFT JOIN Purchase.MiscTypeMaster mt  ON mt.MiscTypeCode  = 'POImage'
            WHERE d.POId = @id
            ORDER BY d.Id;
          
            ";

            var cmd = new CommandDefinition(sql, new
            {
                Id = id,
                Pending = MiscEnumEntity.Pending,
                Approved = MiscEnumEntity.Approved
            }, cancellationToken: ct);
            using var multi = await _conn.QueryMultipleAsync(cmd);

            // 1) Header as DTO
            var header = await multi.ReadFirstOrDefaultAsync<PurchaseOrderServiceDetailDto>();
            if (header is null) return null;

            // 2) Ensure Service PO
            var isService = await multi.ReadFirstOrDefaultAsync<int?>();
            if (isService != 1) return null;

            // 3..6) Children as DTOs
            var svcHeaders = (await multi.ReadAsync<PurchaseOrderServiceHeaderDto>()).ToList();
            var lines = (await multi.ReadAsync<PurchaseOrderServiceLineDto>()).ToList();
            var schedules = (await multi.ReadAsync<PurchaseOrderServiceScheduleDto>()).ToList();
            var terms = (await multi.ReadAsync<PurchaseOrderServicePaymentTermDto>()).ToList();
            var docs = (await multi.ReadAsync<ServiceDocumentDto>()).ToList();

            // Stitch lines under service headers
            var linesByHeader = lines.GroupBy(x => x.ServicePoHeaderId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var sh in svcHeaders)
            {
                sh.PurchaseOrderId = header.Id;
                if (linesByHeader.TryGetValue(sh.Id ?? 0, out var lns))
                {
                    foreach (var ln in lns)
                    {
                        ln.PurchaseOrderId = header.Id;
                        ln.ServicePoHeaderId = sh.Id ?? ln.ServicePoHeaderId;
                    }
                    sh.Lines = lns;
                }
            }

            // Stitch schedules under lines (ServiceItemId = line.Id)
            var schedByLine = schedules.GroupBy(s => s.ServiceItemId).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var ln in lines)
            {
                if (schedByLine.TryGetValue(ln.Id ?? 0, out var sc))
                {
                    foreach (var s in sc)
                    {
                        s.PurchaseOrderId = header.Id;
                        s.ServicePoHeaderId = ln.ServicePoHeaderId;
                        s.ServiceItemId = ln.Id ?? s.ServiceItemId;
                    }
                    ln.Schedules = sc;
                }
            }

            // Attach to header
            header.ServicePo = svcHeaders;
            foreach (var t in terms) t.PurchaseOrderId = header.Id;
            header.PaymentTerms = terms;
            header.DocumentsList = docs;

            // 🔹 Currency enrichment (best-effort)
            if (header.CurrencyId > 0 && string.IsNullOrWhiteSpace(header.Currency))
            {
                try
                {
                    // if API accepts a single Id, many services still want a list
                    var res = await _currencyLookup.GetByIdsAsync(new[] { header.CurrencyId }, ct);
                    var found = res?.FirstOrDefault();
                    if (found is not null)
                    {
                        // choose what you want to display (Code or Name)
                        header.Currency = !string.IsNullOrWhiteSpace(found.Code) ? found.Code : found.Name;
                    }
                }
                catch
                {
                    // swallow enrichment failure
                }
            }

            if (header.VendorId > 0)
            {
                var partyDetails = await _partyLookup.GetByIdAsync(header.VendorId, ct);
                if (partyDetails != null)
                {
                    header.VendorName = partyDetails.PartyName;

                }
            }

            return header;
        }


        public async Task<PurchaseOrderHeader?> GetByIdAsync(int id, CancellationToken ct)
        {
            const string sql = @"
            -- 1) header
            SELECT *
            FROM Purchase.PurchaseOrderHeader
            WHERE Id = @Id;

            -- 2) service headers
            SELECT *
            FROM Purchase.PurchaseOrderServiceHeader
            WHERE PurchaseOrderId = @Id;

            -- 3) service lines
            SELECT *
            FROM Purchase.PurchaseOrderServiceLine
            WHERE PurchaseOrderId = @Id;

            -- 4) schedules
            SELECT *
            FROM Purchase.PurchaseOrderServiceSchedule
            WHERE PurchaseOrderId = @Id;

            -- 5) payment terms
            SELECT *
            FROM Purchase.PurchasePaymentTerm
            WHERE PurchaseOrderId = @Id;
            ";

            using var multi = await _conn.QueryMultipleAsync(sql, new { Id = id });

            var header = await multi.ReadFirstOrDefaultAsync<PurchaseOrderHeader>();
            if (header == null) return null;

            var serviceHeaders = (await multi.ReadAsync<PurchaseOrderServiceHeader>()).ToList();
            var serviceLines = (await multi.ReadAsync<PurchaseOrderServiceLine>()).ToList();
            var schedules = (await multi.ReadAsync<PurchaseOrderServiceSchedule>()).ToList();
            var paymentTerms = (await multi.ReadAsync<PurchasePaymentTerm>()).ToList();

            // stitch: header -> service headers
            header.ServicePos = serviceHeaders;

            // stitch: service header -> lines
            var linesByHeader = serviceLines.GroupBy(l => l.ServicePoHeaderId)
                                            .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var sh in serviceHeaders)
            {
                if (linesByHeader.TryGetValue(sh.Id, out var lines))
                    sh.Items = lines;
                else
                    sh.Items = new List<PurchaseOrderServiceLine>();
            }

            // stitch: line -> schedules
            var schedByLine = schedules.GroupBy(s => s.ServiceItemId)
                                    .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var line in serviceLines)
            {
                if (schedByLine.TryGetValue(line.Id, out var sc))
                    line.PurchaseOrderServiceSchedules = sc;
                else
                    line.PurchaseOrderServiceSchedules = new List<PurchaseOrderServiceSchedule>();
            }

            // payment terms
            header.PaymentTerms = paymentTerms;

            return header;
        }



        public async Task<(List<GetServicePOPendingGroupDto> Rows, int Total)> GetServicePOPendingAsync(
          int? page, int? size, string? search, int? poId, CancellationToken ct)
        {
            var p = (page.HasValue && page > 0) ? page.Value : 1;
            var s = (size.HasValue && size > 0) ? size.Value : 15;
            var off = (p - 1) * s;
            var like = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
            var unitId = _ip.GetUnitId();

            // ApprovalStatus:Pending
            var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

            var sql = @"
                /* 1) Filter matching Service-PO headers */
                CREATE TABLE #filtered (Id INT PRIMARY KEY);
                INSERT INTO #filtered (Id)
                SELECT DISTINCT h.Id
                FROM Purchase.PurchaseOrderHeader h
                JOIN Purchase.PurchaseOrderServiceHeader sh
                    ON sh.PurchaseOrderId = h.Id AND sh.IsDeleted = 0
                WHERE h.IsDeleted = 0
                AND (@UnitId IS NULL OR h.UnitId = @UnitId)
                AND h.StatusId = @StatusId
                AND (@PoId IS NULL OR h.Id = @PoId)
                AND (
                        @Search IS NULL OR @Search = '' OR
                        h.PONumber LIKE @LikeSearch OR
                        EXISTS (
                            SELECT 1
                            FROM Purchase.PurchaseOrderServiceLine sl
                            WHERE sl.PurchaseOrderId = h.Id AND sl.IsDeleted = 0
                            AND (sl.ServiceDescription LIKE @LikeSearch)
                        )
                    );

                /* 2) Page ids (DESC by Id) */
                CREATE TABLE #paged (Id INT PRIMARY KEY);
                WITH numbered AS (
                    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id DESC) rn
                    FROM #filtered
                )
                INSERT INTO #paged (Id)
                SELECT Id FROM numbered WHERE rn BETWEEN (@off + 1) AND (@off + @size);

                /* 3) Header groups (PO + Service header) */
                SELECT
                    h.Id                   ,
                    h.PONumber,
                    h.PODate,
                    h.VendorId,
                    h.PurchaseValue,
                    h.StatusId,
                    h.ItemTotal,
                    h.DiscountTotal,
                    h.PandFTotal,
                    h.MiscCharges,
                    h.GSTTotal,
                    h.CGSTTotal,
                    h.SGSTTotal,
                    h.IGSTTotal,
                    h.FreightTotal,
                    h.InsuranceTotal,
                    h.TDSTotal,
                    h.AdvanceAmount,
                    h.CreatedDate           AS CreatedDate,
                    h.CreatedByName         AS CreatedByName,
                    st.Code                 AS StatusCode,
                    cat.Code                AS POCategoryCode,
                    mth.Code                AS POMethodCode,

                    sh.Id                   AS ServicePoHeaderId,
                    sh.ServiceCategoryId,
                    sh.ContractTypeId,
                    sh.FrequencyId,
                    sh.ValidityFrom,
                    sh.ValidityTo,
                    sh.TotalOccurrences,
                    sh.OverallLimit,
                    sh.TermDescription,
                    sh.POImage,
                    sh.BillingAddress,
                    sh.DeliveryAddress,
                    sh.CostCenterId,
                    sh.FreightCharges,
                    sh.ModeOfDispatchId,
                    sh.TermsId
                FROM Purchase.PurchaseOrderHeader h
                JOIN Purchase.PurchaseOrderServiceHeader sh
                    ON sh.PurchaseOrderId = h.Id AND sh.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster st  ON st.Id  = h.StatusId
                LEFT JOIN Purchase.MiscMaster cat ON cat.Id = h.POCategoryId
                LEFT JOIN Purchase.MiscMaster mth ON mth.Id = h.POMethodId
                WHERE h.Id IN (SELECT Id FROM #paged)
                ORDER BY h.Id DESC;

                /* 4) Lines */
             WITH x AS (
                SELECT
                    sl.Id,
                    sl.PurchaseOrderId,
                    sl.ServicePoHeaderId,
                    ROW_NUMBER() OVER (
                        PARTITION BY sl.PurchaseOrderId
                        ORDER BY sl.Id
                    ) AS LineNumber,           -- <-- renamed from LineNo
                    sl.RequestId,
                    sl.ServiceId,
                    sl.ServiceDescription,
                    sl.UOMId,
                    sl.PlannedQuantity,
                    sl.PlannedRate,
                    sl.DiscountId AS DiscountTypeId,
                    sl.Discount,
                    sl.ItemCost,
                    sl.OtherCost,
                    sl.OtherCharges,
                    sl.GstPercent,
                    sl.Remarks
                FROM Purchase.PurchaseOrderServiceLine sl
                WHERE sl.IsDeleted = 0
                AND sl.PurchaseOrderId IN (SELECT Id FROM #paged)
            )
            SELECT
                Id,
                PurchaseOrderId,
                ServicePoHeaderId,
                LineNumber,                   -- <-- use new alias
                RequestId,
                ServiceId,
                ServiceDescription,
                UOMId,
                PlannedQuantity,
                PlannedRate,
                DiscountTypeId,
                Discount,
                ItemCost,
                OtherCost,
                OtherCharges,
                GstPercent,
                Remarks,
                CAST(0   AS INT)            AS Edit,
                CAST(NULL AS NVARCHAR(250)) AS EditReason
            FROM x
            ORDER BY PurchaseOrderId, LineNumber;  

                /* 5) Total */
                SELECT COUNT(1) FROM #filtered;

                /* 6) Cleanup */
                DROP TABLE #paged;
                DROP TABLE #filtered;"  ;

            var param = new
            {
                UnitId = unitId,
                StatusId = pending.Id,
                PoId = poId,
                Search = search,
                LikeSearch = like,
                off,
                size = s
            };

            using var multi = await _conn.QueryMultipleAsync(
                new CommandDefinition(sql, param, cancellationToken: ct));

            // 3) headers
            var headers = (await multi.ReadAsync<GetServicePOPendingGroupDto>()).ToList();

            // 4) lines
            var lines = (await multi.ReadAsync<GetPOServicePendingDto>()).ToList();

            // 5) total
            var total = await multi.ReadFirstAsync<int>();

            // attach lines to their PO group
            var byPo = headers.ToDictionary(h => h.Id, h => h);
            foreach (var g in headers) g.Lines ??= new List<GetPOServicePendingDto>();

            foreach (var d in lines)
                if (byPo.TryGetValue(d.PurchaseOrderId, out var grp))
                    grp.Lines.Add(d);

            return (headers, total);
        }

        public async Task<ServicePOHeaderForSesDto?> GetServicePOHeaderForSesAsync(int poId, CancellationToken ct)
        {

            const string sql = @"
        SELECT TOP (1)
            h.Id      AS PurchaseOrderId,
            sh.Id     AS ServicePoHeaderId,
            h.PONumber,
            h.PODate,
            h.VendorId,
            h.UnitId,
            h.StatusId
        FROM Purchase.PurchaseOrderHeader h
        JOIN Purchase.PurchaseOrderServiceHeader sh
        ON sh.PurchaseOrderId = h.Id AND sh.IsDeleted = 0
        WHERE h.IsDeleted = 0
        AND h.Id = @PoId;";

            return await _conn.QueryFirstOrDefaultAsync<ServicePOHeaderForSesDto>(
                new CommandDefinition(sql, new { PoId = poId }, cancellationToken: ct));
        }

        public async Task<PoServiceHeaderByIdDto?> GetServicePoHeaderByIdAsync(int poId, CancellationToken ct)
        {
            var unitId = _ip.GetUnitId();

            const string sql = @"
            SELECT
            a.PONumber,
            CAST(a.PODate AS datetime2)     AS  PODate    ,         
            a.VendorId,
            b.ServiceCategoryId,
            sc.Code            AS ServiceCategory, 
            b.ContractTypeId,
            ct.Code          AS ContractType,    
            a.UnitId,
            CAST(b.ValidityFrom AS datetime2)                 AS ValidityFrom, 
            CAST(b.ValidityTo   AS datetime2)                 AS ValidityTo 
                FROM [Purchase].[PurchaseOrderHeader]        AS a
                JOIN [Purchase].[PurchaseOrderServiceHeader] AS b
                ON b.PurchaseOrderId = a.Id
                LEFT JOIN [Purchase].[MiscMaster]            AS sc
                ON sc.Id = b.ServiceCategoryId
                LEFT JOIN [Purchase].[MiscMaster]            AS ct
                ON ct.Id = b.ContractTypeId
                    WHERE a.Id = @Id
                    AND ISNULL(a.IsDeleted, 0) = 0
                    AND ISNULL(b.IsDeleted, 0) = 0;";



            var cmd = new CommandDefinition(sql, new { Id = poId, UnitId = unitId }, cancellationToken: ct);
            return await _conn.QueryFirstOrDefaultAsync<PoServiceHeaderByIdDto>(cmd);
        }
        
        public async Task<List<ServiceScheduleDto>> GetByPoAndServiceIdAsync(
            int purchaseOrderId,
            int serviceId,
            CancellationToken ct)
        {
            // 1️⃣ Check if SES already exists for this PO + Service
            const string sesExistsSql = @"
                SELECT CASE 
                        WHEN EXISTS (
                            SELECT 1
                            FROM Purchase.Purchase.ServiceEntrySheets ses
                            INNER JOIN Purchase.Purchase.PurchaseOrderServiceSchedule sch
                                ON sch.Id = ses.ScheduleId
                                AND ISNULL(sch.IsDeleted, 0) = 0
                            INNER JOIN Purchase.Purchase.PurchaseOrderServiceLine l
                                ON l.Id = sch.ServiceItemId
                                AND ISNULL(l.IsDeleted, 0) = 0
                            WHERE l.PurchaseOrderId = @PurchaseOrderId
                            AND l.ServiceId      = @ServiceId
                            AND ISNULL(ses.IsDeleted, 0) = 0
                        )
                        THEN 1 ELSE 0
                    END AS HasSes;
            ";

            var sesCmd = new CommandDefinition(
                sesExistsSql,
                new { PurchaseOrderId = purchaseOrderId, ServiceId = serviceId },
                cancellationToken: ct);

            var hasSes = await _conn.ExecuteScalarAsync<int>(sesCmd);

            // 2️⃣ If SES exists → return empty list
            if (hasSes == 1)
            {
                return new List<ServiceScheduleDto>();
            }

            // 3️⃣ Else → run your existing schedule query
            const string sql = @"
                SELECT
                    sch.Id,
                    sch.PurchaseOrderId,
                    sch.ServicePoHeaderId,
                    sch.ServiceItemId,
                    sch.ScheduleNo,
                    sch.OccurrencePeriod,
                    sch.OccurrenceDescription,
                    sch.ActivityTypeId,
                    at.Code AS ActivityType,
                    sch.PlannedDurationHrs,
                    CAST(sch.DueDate          AS datetime2) AS DueDate,
                    CAST(sch.ServiceStartDate AS datetime2) AS ServiceStartDate,
                    CAST(sch.ServiceEndDate   AS datetime2) AS ServiceEndDate,
                    sch.PlannedQuantity,
                    sch.PlannedRate,
                    sch.PlannedValue,
                    sch.AutoGenerateSES,
                    sch.Remarks,

                    CASE 
                        WHEN ses.Id IS NULL THEN 'No'
                        ELSE 'Yes'
                    END AS SESAlreadyGenerated

                FROM Purchase.Purchase.PurchaseOrderServiceSchedule AS sch
                JOIN Purchase.Purchase.PurchaseOrderServiceLine    AS l
                    ON l.Id = sch.ServiceItemId

                LEFT JOIN Purchase.Purchase.ServiceEntrySheets AS ses
                    ON ses.PurchaseOrderId   = l.PurchaseOrderId
                AND ses.ScheduleId        = sch.Id
                AND ISNULL(ses.IsDeleted, 0) = 0

                LEFT JOIN Purchase.Purchase.MiscMaster AS at
                    ON at.Id = sch.ActivityTypeId

                WHERE l.PurchaseOrderId     = @PurchaseOrderId
                AND l.ServiceId           = @ServiceId
                AND ISNULL(l.IsDeleted, 0)   = 0
                AND ISNULL(sch.IsDeleted, 0) = 0

                ORDER BY sch.ScheduleNo;
            ";

            var cmd = new CommandDefinition(
                sql,
                new { PurchaseOrderId = purchaseOrderId, ServiceId = serviceId },
                cancellationToken: ct);

            var rows = await _conn.QueryAsync<ServiceScheduleDto>(cmd);
            return rows.ToList();
        }


        // public async Task<List<ServiceScheduleDto>> GetByPoAndServiceIdAsync(int purchaseOrderId, int serviceId, CancellationToken ct)
        // {

        //         const string sql = @"
        //     SELECT
        //         sch.Id,
        //         sch.PurchaseOrderId,
        //         sch.ServicePoHeaderId,
        //         sch.ServiceItemId,
        //         sch.ScheduleNo,
        //         sch.OccurrencePeriod,
        //         sch.OccurrenceDescription,
        //         sch.ActivityTypeId,
        //         at.Code AS ActivityType,             -- if you have misc master for activity
        //         sch.PlannedDurationHrs,
        //         CAST(sch.DueDate          AS datetime2) AS DueDate,
        //         CAST(sch.ServiceStartDate AS datetime2) AS ServiceStartDate,
        //         CAST(sch.ServiceEndDate   AS datetime2) AS ServiceEndDate,
        //         sch.PlannedQuantity,
        //         sch.PlannedRate,
        //         sch.PlannedValue,
        //         sch.AutoGenerateSES,
        //         sch.Remarks,

        //         --  SES flag / info
        //         CASE 
        //             WHEN ses.Id IS NULL THEN 'No'               -- or 'N'
        //             ELSE 'Yes'                                  -- or 'Y'
        //         END AS SESAlreadyGenerated

        //     FROM Purchase.Purchase.PurchaseOrderServiceSchedule AS sch
        //     JOIN Purchase.Purchase.PurchaseOrderServiceLine    AS l
        //         ON l.Id = sch.ServiceItemId

        //     --  Left join SES: if exists => already generated
        //     LEFT JOIN Purchase.Purchase.ServiceEntrySheets AS ses
        //         ON ses.PurchaseOrderId = l.PurchaseOrderId
        //     AND ses.ScheduleId      = sch.Id
        //     AND ISNULL(ses.IsDeleted, 0) = 0

        //     -- (optional) activity type name, if you store in MiscMaster
        //     LEFT JOIN Purchase.Purchase.MiscMaster AS at
        //         ON at.Id = sch.ActivityTypeId

        //     WHERE l.PurchaseOrderId   = @PurchaseOrderId
        //     AND l.ServiceId         = @ServiceId
        //     AND ISNULL(l.IsDeleted, 0)   = 0
        //     AND ISNULL(sch.IsDeleted, 0) = 0

        //     ORDER BY sch.ScheduleNo;";

        //     var cmd = new CommandDefinition(sql, new { PurchaseOrderId = purchaseOrderId, ServiceId = serviceId }, cancellationToken: ct);
        //     var rows = await _conn.QueryAsync<ServiceScheduleDto>(cmd);
        //     return rows.ToList();



        // }


        public async Task<List<PoIdNumberDto>> GetApprovedServicePoAsync()
        {
            var unitId = _ip.GetUnitId();
            const string sql = @"
            SELECT A.Id AS POId, A.PONumber AS ServicePONumber
            FROM [Purchase].[PurchaseOrderHeader] AS A
            INNER JOIN [Purchase].[PurchaseOrderServiceHeader] AS B ON A.Id = B.PurchaseOrderId
            INNER JOIN [Purchase].[MiscMaster] AS C ON C.Id = A.POCategoryId
            INNER JOIN [Purchase].[MiscMaster] AS F ON F.Id = A.StatusId
            WHERE C.[Description] = @PoCategory AND F.[Description] = @Status AND A.IsDeleted = 0
            ORDER BY A.PODate DESC, A.Id DESC;";

            var p = new { PoCategory = MiscEnumEntity.POCategoryService, Status = MiscEnumEntity.Approved, UnitId = unitId };
            var rows = await _conn.QueryAsync<PoIdNumberDto>(sql, p);
            return rows.ToList();
        }

        public async Task<IEnumerable<GetServicePOLinesDto>> GetLinesByPoIdAsync(
       int poId,
       CancellationToken ct)
        {
            var sql = @"
                SELECT 
                    Id,
                    PurchaseOrderId,
                    ServicePoHeaderId,
                    [LineNo],
                    ServiceId,
                    ServiceDescription
                FROM Purchase.PurchaseOrderServiceLine
                WHERE IsDeleted = 0
                  AND PurchaseOrderId = @PoId;";

            var param = new { PoId = poId };

            return await _conn.QueryAsync<GetServicePOLinesDto>(
                new CommandDefinition(sql, param, cancellationToken: ct));
        }

        public async Task<SesFromScheduleRawDto?> GetSesCreateSourceAsync(
        int purchaseOrderId,
        int scheduleNo,
        int serviceItemId,
        CancellationToken ct = default)
        {
            const string sql = @"
            SELECT 
                d.Id                 AS ScheduleId,
                a.Id                 AS PurchaseOrderId,
                a.UnitId             AS UnitId,
                a.PODate             AS PODate,
                a.VendorId           AS VendorId,
                b.ServiceCategoryId  AS ServiceCategoryId,
                b.ContractTypeId     AS ContractTypeId,
                b.ValidityFrom       AS ValidityFrom,
                b.ValidityTo         AS ValidityTo,
                c.PlannedQuantity    AS PlannedQuantity,
                c.PlannedRate        AS PlannedRate,
                c.GstPercent         AS GstPercent,
                c.UOMId              AS UOMId,
                c.DiscountId       AS DiscountTypeId,
                c.Discount           AS DiscountValue,     -- alias fix
                c.[LineNo]           AS LineNumber,        -- alias fix
                c.ServiceId          AS ServiceId,
                e.ServiceCode        AS ServiceCode,
                e.ServiceDescription AS ServiceDescription,
                d.Id                 AS ScheduleId,
                d.ScheduleNo         AS OccurrenceNo,
                d.OccurrencePeriod   AS OccurrencePeriod,
                d.ServiceStartDate   AS ScheduleStartDate, -- alias fix
                d.ServiceEndDate     AS ScheduleEndDate,   -- alias fix
                d.AutoGenerateSES    AS AutoGenerateSES,
                d.Remarks            AS LineRemarks        -- alias fix
            FROM Purchase.Purchase.PurchaseOrderHeader a  
            LEFT JOIN Purchase.Purchase.PurchaseOrderServiceHeader  b 
                   ON a.Id = b.PurchaseOrderId 
            LEFT JOIN Purchase.Purchase.PurchaseOrderServiceLine   c  
                   ON a.Id = c.PurchaseOrderId  
            LEFT JOIN Purchase.Purchase.PurchaseOrderServiceSchedule d  
                   ON a.Id = d.PurchaseOrderId  
                  AND c.Id = d.ServiceItemId     
                  AND d.ScheduleNo = @ScheduleNo
            LEFT JOIN Purchase.Purchase.ServiceMaster   e    
                   ON c.ServiceId = e.Id  
            WHERE a.Id = @PurchaseOrderId
              AND d.ServiceItemId = @ServiceItemId
              AND ISNULL(a.IsDeleted,0) = 0
              AND ISNULL(c.IsDeleted,0) = 0
              AND ISNULL(d.IsDeleted,0) = 0;";

            var param = new
            {
                PurchaseOrderId = purchaseOrderId,
                ScheduleNo = scheduleNo,
                ServiceItemId = serviceItemId
            };

            var raw = await _conn.QueryFirstOrDefaultAsync<SesFromScheduleRawDto>(
                new CommandDefinition(sql, param, cancellationToken: ct));

            return raw; // ✅ matches interface return type
        }



        public async Task<GetServiceEntrySheetDto?> GetSESByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
        -- header
        SELECT 
            s.Id, s.SESDate, s.SESStatusId, s.PurchaseOrderId, s.PODate, s.VendorId,
            s.ServiceCategoryId, s.ContractTypeId, s.ValidityFrom, s.ValidityTo,
            s.UnitId, s.AttachmentFileName, s.ServiceId, s.ServiceDescription,
            s.ScheduleId, s.OccurrenceNo, s.OccurrencePeriod, s.ScheduleStartDate, s.ScheduleEndDate,
            s.ActualQuantity, s.ActualRate, s.ActualValue, s.DiscountTypeId, s.DiscountValue,
            s.TaxPercentage, s.TaxValue, s.TotalValue, s.WorkStartDate, s.WorkEndDate, s.DurationHrs,
            s.LineRemarks, s.StatusId
        FROM Purchase.Purchase.ServiceEntrySheets s
        WHERE s.Id = @Id;

        -- activities
        SELECT 
            a.Id, a.EntrySheetId, a.ActivityTypeId, a.Description,
             a.PerformedByName, a.SESActivityStatusId, a.StatusRemarks,
            a.CreatedDate, a.CreatedByName
        FROM Purchase.Purchase.ServiceEntryActivities a
        WHERE a.EntrySheetId = @Id
        ORDER BY a.CreatedDate DESC;

        
        ";

            using var grid = await _conn.QueryMultipleAsync(sql, new { Id = id });
            var header = await grid.ReadFirstOrDefaultAsync<GetServiceEntrySheetDto>();
            if (header is null) return null;

            header.Activities = (await grid.ReadAsync<ServiceEntryActivityDto>()).ToList();
            return header;
        }

        public async Task<List<SesApprovalListDto>> GetServiceEntrySheetsForApprovalAsync(DateTimeOffset? fromDate, DateTimeOffset? toDate, int? vendorId, CancellationToken ct = default)
        {

            var unitId = _ip.GetUnitId();

            var sql = new StringBuilder(@"
                SELECT
                    Id
                    , SESDate
                    , PurchaseOrderId
                    , PODate
                    , VendorId
                    , NULL        AS VendorName          
                    , ServiceDescription
                    , ScheduleId
                    , OccurrenceNo
                    , OccurrencePeriod
                    , ActualQuantity
                    , ActualRate
                    , ActualValue
                    , TaxPercentage
                    , TaxValue
                    , TotalValue
                    , LineRemarks
                    , SESStatusId
                    , StatusId
                    , UnitId
                    , CreatedByName
                    , CreatedDate
                FROM Purchase.Purchase.ServiceEntrySheets
                WHERE UnitId = @UnitId
                AND IsActive = 1
                AND IsDeleted = 0
                AND StatusId = @StatusId
                ");

            var parameters = new DynamicParameters();

            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.ApprovalStatus,
                MiscEnumEntity.Pending);

            parameters.Add("@UnitId", unitId, DbType.Int32);
            parameters.Add("@StatusId", pendingStatus.Id, DbType.Int32);


            if (fromDate.HasValue)
            {
                sql.AppendLine("  AND SESDate >= @FromDate");
                parameters.Add("@FromDate", fromDate.Value, DbType.DateTimeOffset);
            }

            if (toDate.HasValue)
            {
                sql.AppendLine("  AND SESDate <= @ToDate");
                parameters.Add("@ToDate", toDate.Value, DbType.DateTimeOffset);
            }

            if (vendorId.HasValue)
            {
                sql.AppendLine("  AND VendorId = @VendorId");
                parameters.Add("@VendorId", vendorId.Value, DbType.Int32);
            }

            sql.AppendLine("ORDER BY SESDate DESC, Id DESC;");

            var result = await _conn.QueryAsync<SesApprovalListDto>(
                sql.ToString(),
                parameters);

            return result.AsList();

        }

        public async Task<IEnumerable<ServiceEntrySheetWithActivitiesDto>> GetByPurchaseOrderIdAsync(
            int purchaseOrderId,
            CancellationToken cancellationToken = default)
        {
            const string sesSql = @"
                SELECT 
                    s.Id, 
                    s.SESDate, 
                    s.SESStatusId, 
                    s.PurchaseOrderId, 
                    s.PODate, 
                    s.VendorId,
                    s.ServiceCategoryId, 
                    s.ContractTypeId, 
                    s.ValidityFrom, 
                    s.ValidityTo,
                    s.UnitId, 
                    s.AttachmentFileName, 
                    s.ServiceId,
                    sm.ServiceCode, 
                    s.ServiceDescription,
                    s.ScheduleId, 
                    s.OccurrenceNo, 
                    s.OccurrencePeriod, 
                    s.ScheduleStartDate, 
                    s.ScheduleEndDate,
                    s.ActualQuantity, 
                    s.ActualRate, 
                    s.ActualValue, 
                    s.DiscountTypeId, 
                    s.DiscountValue,
                    s.TaxPercentage, 
                    s.TaxValue, 
                    s.TotalValue, 
                    s.WorkStartDate, 
                    s.WorkEndDate, 
                    s.DurationHrs,
                    s.LineRemarks, 
                    s.StatusId
                FROM Purchase.Purchase.ServiceEntrySheets s 
                inner join Purchase.ServiceMaster sm  on s.ServiceId=sm.Id               
                WHERE s.PurchaseOrderId = @PurchaseOrderId;";

            var sesList = (await _conn.QueryAsync<ServiceEntrySheetWithActivitiesDto>(
                    sesSql,
                    new { PurchaseOrderId = purchaseOrderId }))
                .ToList();

            if (!sesList.Any())
                return sesList;

            var entrySheetIds = sesList
                .Select(s => s.Id)
                .Distinct()
                .ToArray();

            const string activitiesSql = @"
                SELECT 
                    a.Id,
                    a.EntrySheetId,
                    a.ActivityTypeId,
                    a.Description,                    
                    a.PerformedByName,
                    a.SESActivityStatusId,
                    a.StatusRemarks,
                    a.CreatedDate,
                    a.CreatedByName
                FROM Purchase.Purchase.ServiceEntryActivities a
                WHERE a.EntrySheetId IN @EntrySheetIds
                ORDER BY a.EntrySheetId, a.CreatedDate DESC;";

            // ⬇️ Use GetAllServiceEntryActivityDto here ⬇️
            var activities = (await _conn.QueryAsync<GetAllServiceEntryActivityDto>(
                    activitiesSql,
                    new { EntrySheetIds = entrySheetIds }))
                .ToList();

            var activityLookup = activities.ToLookup(a => a.EntrySheetId);

            // 🔹 Documents
                const string documentsSql = @"
                    SELECT 
                        d.Id,
                        d.ServiceEntrySheetId,
                        d.DocumentId,
                        d.FileName,
                        d.UploadedDate,
                        d.UploadedPath,
                        d.DocumentName
                    FROM Purchase.Purchase.ServiceEntrySheetDocuments d
                    WHERE d.ServiceEntrySheetId IN @EntrySheetIds
                    AND d.IsDeleted = 0;";

                var documents = (await _conn.QueryAsync<ServiceEntrySheetDocumentDto>(
                        documentsSql,
                        new { EntrySheetIds = entrySheetIds }))
                    .ToList();

                var documentLookup = documents.ToLookup(d => d.ServiceEntrySheetId);

            foreach (var ses in sesList)
            {
                ses.Activities = activityLookup[ses.Id].ToList();

                ses.Documents  = documentLookup[ses.Id].ToList();
            }

            return sesList;
        }

        public async Task<(List<GetServiceEntrySheetListDto> Rows, int Total)>
            GetAllServiceEntrySheetsAsync(int pageNumber, int pageSize, string? searchTerm)
        {

            // 1️⃣ Resolve "Pending" status from MiscMaster via EnumEntity
            var statusPending = await _miscMasterQueryRepository
                .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
            var sql = $$"""
                DECLARE @TotalCount INT;

                -- total count
                SELECT @TotalCount = COUNT(*)
                FROM [Purchase].[ServiceEntrySheets] S
                WHERE S.IsDeleted = 0
                {{(string.IsNullOrWhiteSpace(searchTerm)
                    ? ""
                    : "AND (S.ServiceDescription LIKE @Search OR CAST(S.PurchaseOrderId AS nvarchar(50)) LIKE @Search)")}};

                -- page
                SELECT 
                    S.Id, 
                    S.SESDate, 
                    S.SESStatusId,
                    SES.Code AS SESStatus, 
                    S.PurchaseOrderId, 
                    S.PODate, 
                    S.VendorId,
                    S.ServiceCategoryId, 
                    S.ContractTypeId, 
                    S.ValidityFrom, 
                    S.ValidityTo,
                    S.UnitId, 
                    S.AttachmentFileName, 
                    S.ServiceId, 
                    S.ServiceDescription,
                    S.ScheduleId, 
                    S.OccurrenceNo, 
                    S.OccurrencePeriod, 
                    S.ScheduleStartDate, 
                    S.ScheduleEndDate,
                    S.ActualQuantity, 
                    S.ActualRate, 
                    S.ActualValue, 
                    S.DiscountTypeId, 
                    S.DiscountValue,
                    S.TaxPercentage, 
                    S.TaxValue, 
                    S.TotalValue, 
                    S.WorkStartDate, 
                    S.WorkEndDate, 
                    S.DurationHrs,
                    S.LineRemarks, 
                    S.StatusId,
                    SM.Code AS Status
                FROM [Purchase].[ServiceEntrySheets] S
                Inner JOIN [Purchase].[PurchaseOrderHeader] POH ON POH.Id = S.PurchaseOrderId
                LEFT JOIN [Purchase].[MiscMaster] SM ON SM.Id = S.StatusId
                LEFT JOIN [Purchase].[MiscMaster] SES ON SES.Id = S.SESStatusId
                WHERE S.IsDeleted = 0 AND POH.IsDeleted = 0  
                 AND S.StatusId = @StatusId
                {{(string.IsNullOrWhiteSpace(searchTerm)
                    ? ""
                    : "AND (S.ServiceDescription LIKE @Search OR CAST(S.PurchaseOrderId AS nvarchar(50)) LIKE @Search)")}}
                ORDER BY S.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
                """;
           

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize,
                StatusId = statusPending.Id
     

            };

            using var grid = await _conn.QueryMultipleAsync(sql, parameters);

            var rows = (await grid.ReadAsync<GetServiceEntrySheetListDto>()).ToList();
            var total = await grid.ReadFirstAsync<int>();

            return (rows, total);
        }
        



           // 🔹 1) SES by Id // 🔹 1) SES by Id
        public async Task<ServiceEntrySheetDetailDto.SesDto?> GetSesByIdAsync(
            int sesId,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT 
                    s.Id,
                    s.SESDate,
                    s.SESStatusId,
                    s.PurchaseOrderId,
                    s.PODate,
                    s.VendorId,
                    s.ServiceCategoryId,
                    s.ContractTypeId,
                    s.ValidityFrom,
                    s.ValidityTo,
                    s.UnitId,
                    s.AttachmentFileName,
                    s.ServiceId,
                    s.ServiceDescription,
                    s.ScheduleId,
                    s.OccurrenceNo,
                    s.OccurrencePeriod,
                    s.ScheduleStartDate,
                    s.ScheduleEndDate,
                    s.ActualQuantity,
                    s.ActualRate,
                    s.ActualValue,
                    s.DiscountTypeId,
                    s.DiscountValue,
                    s.TaxPercentage,
                    s.TaxValue,
                    s.TotalValue,
                    s.WorkStartDate,
                    s.WorkEndDate,
                    s.DurationHrs,
                    s.LineRemarks,
                    s.StatusId
                FROM Purchase.Purchase.ServiceEntrySheets s
                WHERE s.Id = @SesId AND s.IsDeleted = 0;";

            return await _conn.QueryFirstOrDefaultAsync<ServiceEntrySheetDetailDto.SesDto>(
                sql,
                new { SesId = sesId });
        }

        // 🔹 2) SES Activities
        public async Task<List<ServiceEntrySheetDetailDto.ActivityDto>> GetSesActivitiesAsync(
            int sesId,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    a.Id,
                    a.EntrySheetId,
                    a.ActivityTypeId,
                    a.Description,
                    a.PerformedByName,
                    a.SESActivityStatusId,
                    a.StatusRemarks,
                    a.IsActive,
                    a.IsDeleted,
                    a.CreatedBy,
                    a.CreatedDate,
                    a.CreatedByName,
                    a.CreatedIP,
                    a.ModifiedBy,
                    a.ModifiedDate,
                    a.ModifiedByName,
                    a.ModifiedIP
                FROM Purchase.Purchase.ServiceEntryActivities a
                WHERE a.EntrySheetId = @SesId
                  AND a.IsDeleted = 0
                ORDER BY a.CreatedDate DESC;";

            var rows = await _conn.QueryAsync<ServiceEntrySheetDetailDto.ActivityDto>(
                sql,
                new { SesId = sesId });

            return rows.ToList();
        }

        public async Task<List<ServiceEntrySheetDetailDto.DocumentDto>> GetserviceEntrySheetDocumentDtosGetSesByIdAsync(
            int sesId,
            CancellationToken ct = default)
        {
            const string sql = @"
              SELECT 
                d.Id,
                d.ServiceEntrySheetId,
                d.DocumentId,
                mm.Code AS DocumentName,
                d.FileName,
                d.UploadedDate,
                mtm.Description + '/' + mt.Description + '/' + d.FileName AS UploadedPath,
                d.IsActive,
                d.IsDeleted,
                d.CreatedBy,
                d.CreatedDate,
                d.CreatedByName,
                d.CreatedIP,
                d.ModifiedBy,
                d.ModifiedDate,
                d.ModifiedByName,
                d.ModifiedIP
            FROM Purchase.ServiceEntrySheetDocuments AS d WITH (NOLOCK)
            JOIN Purchase.MiscMaster AS mm 
                ON mm.Id = d.DocumentId
            LEFT JOIN Purchase.MiscTypeMaster mtm 
                ON mtm.MiscTypeCode = 'ImagePath'
            LEFT JOIN Purchase.MiscTypeMaster mt  
                ON mt.MiscTypeCode = 'POImage'
            WHERE d.ServiceEntrySheetId = @SesId
            AND d.IsDeleted = 0
            ORDER BY d.CreatedDate DESC;";

            var rows = await _conn.QueryAsync<ServiceEntrySheetDetailDto.DocumentDto>(
                sql,
                new { SesId = sesId });

            return rows.ToList();
        }
      

        // 🔹 3) PO Header
        public async Task<ServiceEntrySheetDetailDto.PurchaseOrderHeaderDto?> GetPoHeaderByIdAsync(
            int poId,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT 
                    h.Id,
                    h.UnitId,
                    h.PONumber,
                    h.PODate,
                    h.POCategoryId,
                    h.POMethodId,
                    h.CurrencyId,
                    h.VendorId,
                    h.ItemTotal,
                    h.DiscountTotal,
                    h.PandFTotal,
                    h.MiscCharges,
                    h.GSTTotal,
                    h.CGSTTotal,
                    h.SGSTTotal,
                    h.IGSTTotal,
                    h.FreightTotal,
                    h.InsuranceTotal,
                    h.TDSTotal,
                    h.AdvanceAmount,
                    h.PurchaseValue,
                    h.StatusId,
                    h.IsActive,
                    h.IsDeleted,
                    h.CreatedBy,
                    h.CreatedDate,
                    h.CreatedByName,
                    h.CreatedIP,
                    h.ModifiedBy,
                    h.ModifiedDate,
                    h.ModifiedByName,
                    h.ModifiedIP,
                    h.AmendmentReason,
                    h.OldPOId,
                    h.RevisionNo,
                    h.CapitalTypeId,
                    h.CostCenterId,
                    h.ProjectId,
                    h.PurchaseTypeId
                FROM Purchase.Purchase.PurchaseOrderHeader h
                WHERE h.Id = @PoId AND h.IsDeleted = 0;";

            return await _conn.QueryFirstOrDefaultAsync<ServiceEntrySheetDetailDto.PurchaseOrderHeaderDto>(
                sql,
                new { PoId = poId });
        }

        // 🔹 4) Payment Terms by PO
        public async Task<List<ServiceEntrySheetDetailDto.PaymentTermDto>> GetPaymentTermsByPoIdAsync(
            int poId,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    p.Id,
                    p.PurchaseOrderId,
                    p.PaymentTermId,
                    p.AdvancePercent,
                    p.CreditDays,
                    p.PaymentModelId,
                    p.InsuranceId,
                    p.InsurancePercent,
                    p.InsuranceAmount,
                    p.AdvanceAmount,
                    p.BalancePercent,
                    p.BalanceAmount,
                    p.IsActive,
                    p.IsDeleted,
                    p.CreatedBy,
                    p.CreatedDate,
                    p.CreatedByName,
                    p.CreatedIP,
                    p.ModifiedBy,
                    p.ModifiedDate,
                    p.ModifiedByName,
                    p.ModifiedIP
                FROM Purchase.Purchase.PurchasePaymentTerm p
                WHERE p.PurchaseOrderId = @PoId AND p.IsDeleted = 0;";

                var rows = await _conn.QueryAsync<ServiceEntrySheetDetailDto.PaymentTermDto>(
                    sql,
                    new { PoId = poId });

            return rows.ToList();
        }

        // 🔹 5) Service Headers by PO
        public async Task<List<ServiceEntrySheetDetailDto.ServiceHeaderDto>> GetServiceHeadersByPoIdAsync(
            int poId,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    sh.Id,
                    sh.PurchaseOrderId,
                    sh.ServiceCategoryId,
                    sh.ContractTypeId,
                    sh.FrequencyId,
                    sh.ValidityFrom,
                    sh.ValidityTo,
                    sh.TotalOccurrences,
                    sh.OverallLimit,
                    sh.TermDescription,
                    sh.POImage,
                    sh.IsActive,
                    sh.IsDeleted,
                    sh.CreatedBy,
                    sh.CreatedDate,
                    sh.CreatedByName,
                    sh.CreatedIP,
                    sh.ModifiedBy,
                    sh.ModifiedDate,
                    sh.ModifiedByName,
                    sh.ModifiedIP,
                    sh.BillingAddress,
                    sh.DeliveryAddress,
                    sh.CostCenterId,
                    sh.FreightCharges,
                    sh.ModeOfDispatchId,
                    sh.TermsId
                FROM Purchase.Purchase.PurchaseOrderServiceHeader sh
                WHERE sh.PurchaseOrderId = @PoId AND sh.IsDeleted = 0;";

            var rows = await _conn.QueryAsync<ServiceEntrySheetDetailDto.ServiceHeaderDto>(
                sql,
                new { PoId = poId });

            return rows.ToList();
        }

        // 🔹 6) Service Lines by PO + ServiceId
        public async Task<List<ServiceEntrySheetDetailDto.ServiceLineDto>> GetServiceLinesByPoAndServiceAsync(
            int poId,
            int serviceId,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    sl.Id,
                    sl.PurchaseOrderId,
                    sl.ServicePoHeaderId,
                    sl.[LineNo],
                    sl.RequestId,
                    sl.ServiceId,
                    sl.ServiceDescription,
                    sl.UOMId,
                    sl.PlannedQuantity,
                    sl.PlannedRate,
                    sl.DiscountId AS  DiscountTypeId,
                    sl.Discount,
                    sl.ItemCost,
                    sl.OtherCost,
                    sl.OtherCharges,
                    sl.GstPercent,
                    sl.Remarks,
                    sl.IsActive,
                    sl.IsDeleted,
                    sl.CreatedBy,
                    sl.CreatedDate,
                    sl.CreatedByName,
                    sl.CreatedIP,
                    sl.ModifiedBy,
                    sl.ModifiedDate,
                    sl.ModifiedByName,
                    sl.ModifiedIP,
                    sl.DiscountId,
                    sl.ServiceCode,
                    sl.PlannedValue
                FROM Purchase.Purchase.PurchaseOrderServiceLine sl
                WHERE sl.PurchaseOrderId = @PoId
                  AND sl.ServiceId      = @ServiceId
                  AND sl.IsDeleted      = 0;";

            var rows = await _conn.QueryAsync<ServiceEntrySheetDetailDto.ServiceLineDto>(
                sql,
                new { PoId = poId, ServiceId = serviceId });

            return rows.ToList();
        }

        // 🔹 7) Service Schedule by PO + ScheduleId
        public async Task<List<ServiceEntrySheetDetailDto.ServiceScheduleDto>> GetServiceSchedulesByPoAndScheduleAsync(
            int poId,
            int scheduleId,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    sc.Id,
                    sc.PurchaseOrderId,
                    sc.ServicePoHeaderId,
                    sc.ServiceItemId,
                    sc.ScheduleNo,
                    sc.OccurrencePeriod,
                    sc.OccurrenceDescription,
                    sc.ActivityTypeId,
                    sc.PlannedDurationHrs,
                    sc.DueDate,
                    sc.ServiceStartDate,
                    sc.ServiceEndDate,
                    sc.PlannedQuantity,
                    sc.PlannedRate,
                    sc.PlannedValue,
                    sc.AutoGenerateSES,
                    sc.Remarks,
                    sc.IsActive,
                    sc.IsDeleted,
                    sc.CreatedBy,
                    sc.CreatedDate,
                    sc.CreatedByName,
                    sc.CreatedIP,
                    sc.ModifiedBy,
                    sc.ModifiedDate,
                    sc.ModifiedByName,
                    sc.ModifiedIP
                FROM Purchase.Purchase.PurchaseOrderServiceSchedule sc
                WHERE sc.PurchaseOrderId = @PoId
                  AND sc.Id             = @ScheduleId
                  AND sc.IsDeleted      = 0;";

            var rows = await _conn.QueryAsync<ServiceEntrySheetDetailDto.ServiceScheduleDto>(
                sql,
                new { PoId = poId, ScheduleId = scheduleId });

            return rows.ToList();
        }

        
    }
}
