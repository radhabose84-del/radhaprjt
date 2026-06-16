namespace WarehouseManagement.QATests.Tests.WarehouseMaster;

// ─────────────────────────────────────────────────────────────────────────────
// WarehouseMaster — live-server QA suite (negatives ACTIVE; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-16):
//   POST   /api/WarehouseMaster              CreateWarehouseMasterCommand (MANY FKs — see below)
//   PUT    /api/WarehouseMaster/update       UpdateWarehouseMasterCommand { id, ..., isActive }
//   DELETE /api/WarehouseMaster?id={id}      (id bound from QUERY — action param `int id`)
//   GET    /api/WarehouseMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/WarehouseMaster/{id}         (returns 200 + data:null when missing — NO 404 guard)
//   GET    /api/WarehouseMaster/by-name?name=
//   GET    /api/WarehouseMaster/by-unit/{unitId}
//   GET    /api/WarehouseMaster/Get Parent Warehouse   (literal path WITH a space → URL-encode "Get%20Parent%20Warehouse")
//
// Create requires: warehouseName(req,unique), unitId(req→/api/Unit),
//   warehouseTypeId+storageTypeId+areaTypeId+operationTypeId (req, warehouse-type MiscMaster values),
//   departmentId(req→/api/Department), capacityUOMId(req, UOM), cityId/stateId/countryId,
//   city/state/country NAME strings, addressLine1(req), pincode(req, exactly 6 digits),
//   maxCapacity(req,>0).
//
// Why create-happy + lifecycle are SKIPPED:
//   The four warehouse-type MiscMaster ids (WarehouseType/StorageType/AreaType/OperationType) and a
//   capacity-UOM id are not resolvable on the QA clone (no warehouse-misc list endpoint to discover
//   genuinely-valid ids), and the clone has 0 warehouses for testsales. These are attribute-level
//   [Fact(Skip=...)] — explicit pending work, not silent gaps. Negatives (empty body / missing
//   required / pincode format / missing-FK / no-auth), smoke GetAll, AutoComplete and the extra
//   read reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("WarehouseMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class WarehouseMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/WarehouseMaster";

    private static string _createdName = string.Empty;

    public WarehouseMasterQATests(QAServerFixture fixture) => _f = fixture;

    private string NewName() => "QA WH " + _f.EntityCode[..10];

    // Builds a "best-effort valid" create body; FK ids resolved at runtime where an endpoint exists.
    private static object BuildCreateBody(string name, int unitId, int departmentId, int stateId, int countryId, int cityId) => new
    {
        warehouseName = name,
        unitId,
        isGroup = false,
        isVirtualWarehouse = false,
        warehouseTypeId = 1,
        departmentId,
        storageTypeId = 1,
        areaTypeId = 1,
        operationTypeId = 1,
        capacityUOMId = 1,
        contactPersonName = "QA Person",
        mobileNumber = "9876543210",
        email = "qa@example.com",
        addressLine1 = "QA Address Line 1",
        city = "QA City",
        state = "QA State",
        country = "India",
        cityId,
        stateId,
        countryId,
        pincode = "560001",
        isScrapWarehouse = false,
        isTransitWarehouse = false,
        maxCapacity = 1000m,
        isDefaultStockEntry = false,
        allowedItemGroupIds = new int[] { }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: WarehouseType/StorageType/AreaType/OperationType MiscMaster + capacity-UOM ids not resolvable on clone (no warehouse-misc list endpoint); clone has 0 warehouses for testsales"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdName = NewName();

        var unitId = await QAHelper.FirstIdAsync(_f.Client, "/api/Unit");
        var departmentId = await QAHelper.FirstIdAsync(_f.Client, "/api/Department");
        var stateId = await QAHelper.FirstIdAsync(_f.Client, "/api/State");
        var countryId = await QAHelper.FirstIdAsync(_f.Client, "/api/Country");

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute,
            BuildCreateBody(_createdName, unitId, departmentId, stateId, countryId, _f.CityId));

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute,
            BuildCreateBody("QA No Auth WH", 1, 1, 1, 1, 1));

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MissingWarehouseName_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            warehouseName = "",
            unitId = 1,
            warehouseTypeId = 1,
            departmentId = 1,
            storageTypeId = 1,
            areaTypeId = 1,
            operationTypeId = 1,
            capacityUOMId = 1,
            addressLine1 = "QA Address",
            city = "QA City",
            state = "QA State",
            country = "India",
            cityId = 1,
            stateId = 1,
            countryId = 1,
            pincode = "560001",
            maxCapacity = 100m
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_PincodeInvalid_Returns400()
    {
        // Pincode must be exactly 6 digits — "123" fails the format rule.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            warehouseName = NewName(),
            unitId = 1,
            warehouseTypeId = 1,
            departmentId = 1,
            storageTypeId = 1,
            areaTypeId = 1,
            operationTypeId = 1,
            capacityUOMId = 1,
            addressLine1 = "QA Address",
            city = "QA City",
            state = "QA State",
            country = "India",
            cityId = 1,
            stateId = 1,
            countryId = 1,
            pincode = "123",
            maxCapacity = 100m
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_MissingFKs_Returns400()
    {
        // Only a name supplied — all required FKs (unit/type/department/uom/city/state/country) missing.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            warehouseName = NewName()
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke; tolerant 200/404 — clone has 0 warehouses for testsales)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (no null guard → 200 + data:null for missing)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentId_Returns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — EXTRA READS  (by-unit + Get Parent Warehouse reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_GetByUnit_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-unit/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_GetByUnit_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-unit/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_GetParentWarehouse_Reachable_Returns200Or404()
    {
        // Literal route path contains a space — URL-encode it.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/Get%20Parent%20Warehouse");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_GetParentWarehouse_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/Get%20Parent%20Warehouse");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — AUTOCOMPLETE  (param is `name`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_AutoComplete_WithName_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — UPDATE  (lifecycle BLOCKED — depends on a created id; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a created WarehouseMaster id (TC001 is blocked on warehouse-misc + UOM seeds)"), TestPriority(60)]
    public async Task TC060_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var unitId = await QAHelper.FirstIdAsync(_f.Client, "/api/Unit");
        var departmentId = await QAHelper.FirstIdAsync(_f.Client, "/api/Department");
        var stateId = await QAHelper.FirstIdAsync(_f.Client, "/api/State");
        var countryId = await QAHelper.FirstIdAsync(_f.Client, "/api/Country");

        var body = new
        {
            id = _f.CreatedId,
            warehouseName = "QA Updated WH",
            unitId,
            isGroup = false,
            isVirtualWarehouse = false,
            warehouseTypeId = 1,
            departmentId,
            storageTypeId = 1,
            areaTypeId = 1,
            operationTypeId = 1,
            capacityUOMId = 1,
            addressLine1 = "QA Updated Address",
            city = "QA City",
            state = "QA State",
            country = "India",
            cityId = _f.CityId,
            stateId,
            countryId,
            pincode = "560001",
            maxCapacity = 2000m,
            isActive = 1
        };

        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/update", body);
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(61)]
    public async Task TC061_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/update", new
        {
            id = 999999,
            warehouseName = "QA Updated WH",
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(62)]
    public async Task TC062_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/update", new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — DELETE  (lifecycle BLOCKED; negatives ACTIVE — id bound from QUERY)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Reachable()
    {
        // Delete handler has no explicit not-found guard in the controller — tolerate 200/400/404.
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact(Skip = "needs seeded data: a created WarehouseMaster id (TC001 is blocked on warehouse-misc + UOM seeds)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
