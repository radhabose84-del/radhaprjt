namespace WarehouseManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-WH-01 — Warehouse → rack → bin hierarchy
//   As a warehouse administrator I set up a warehouse, add a rack, and add a bin.
//
// PARTIAL: the read/reachability surface is ACTIVE; the create chain is [Fact(Skip=…)] —
// the QA clone has 0 warehouses for testsales and WarehouseMaster create needs warehouse-type
// MiscMaster ids (WarehouseType/StorageType/AreaType/OperationType) + a capacity UOM that are
// not resolvable from a list endpoint on the clone. Un-skip once that reference data is seeded.
//
// Routes verified from WarehouseManagement.QATests:
//   WarehouseMaster: /api/WarehouseMaster (GET ""/{id}/by-name/by-unit/{unitId}/"Get Parent Warehouse"; DELETE ?id=)
//   RackMaster     : /api/rackmaster (POST create; DELETE ?id=)
//   BinMaster      : /BinMaster (NO /api prefix; POST create; DELETE ?id=)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-WH-01-WarehouseHierarchy")]
[Trait("Module", "WarehouseManagement")]
[Trait("Story", "US-WH-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_WH_01_WarehouseHierarchy_Tests
{
    private readonly QAServerFixture _f;

    private const string WarehouseRoute = "/api/WarehouseMaster";
    private const string RackRoute       = "/api/rackmaster";
    private const string BinRoute        = "/BinMaster";

    private static int _warehouseId;

    public US_WH_01_WarehouseHierarchy_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the warehouse read surface (list + parent + by-unit) is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_WarehouseReads_AreReachable()
    {
        var list = await _f.Client.GetAsync($"{WarehouseRoute}?PageNumber=1&PageSize=15");
        ((int)list.StatusCode).Should().BeOneOf(200, 404);

        var parents = await _f.Client.GetAsync($"{WarehouseRoute}/Get%20Parent%20Warehouse");
        ((int)parents.StatusCode).Should().BeOneOf(200, 404);

        var byUnit = await _f.Client.GetAsync($"{WarehouseRoute}/by-unit/1");
        ((int)byUnit.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC1 (cont.) — rack + bin list reads are reachable.
    [Fact, TestPriority(2)]
    public async Task Step2_RackAndBinReads_AreReachable()
    {
        ((int)(await _f.Client.GetAsync($"{RackRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{BinRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
    }

    // AC1 (cont.) — no-auth is rejected on the warehouse list.
    [Fact, TestPriority(3)]
    public async Task Step3_WarehouseList_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{WarehouseRoute}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(resp);
    }

    // AC2 — create a WarehouseMaster. BLOCKED: needs warehouse-type MiscMaster + UOM ids on the clone.
    [Fact(Skip = "needs seeded data: WarehouseType/StorageType/AreaType/OperationType MiscMaster + capacity UOM ids (not resolvable from a list endpoint on BannariERP_QATest); clone also has 0 warehouses for testsales."), TestPriority(4)]
    public async Task Step4_CreateWarehouse()
    {
        // When seeded: POST /api/WarehouseMaster with the resolved type/storage/area/operation + UOM +
        // location ids + pincode "560001" + maxCapacity, capture _warehouseId.
        await Task.CompletedTask;
    }

    // AC3 — create a RackMaster under the warehouse (depends on Step4).
    [Fact(Skip = "needs seeded data: depends on a WarehouseMaster id (Step4 blocked)."), TestPriority(5)]
    public async Task Step5_CreateRack_UnderWarehouse()
    {
        if (_warehouseId == 0) return;
        await Task.CompletedTask;
    }

    // AC4 — create a BinMaster in the warehouse (depends on Step4).
    [Fact(Skip = "needs seeded data: depends on a WarehouseMaster id (Step4 blocked)."), TestPriority(6)]
    public async Task Step6_CreateBin_InWarehouse()
    {
        if (_warehouseId == 0) return;
        await Task.CompletedTask;
    }
}
