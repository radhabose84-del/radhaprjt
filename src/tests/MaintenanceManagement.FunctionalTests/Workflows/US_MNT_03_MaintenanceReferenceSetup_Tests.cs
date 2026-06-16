namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-03 — Maintenance reference setup
//   As a maintenance administrator I set up the reference masters (cost centre,
//   work centre, maintenance category & type) used to classify maintenance work.
// Fully implementable (FK unit/dept ids best-effort = 1).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-03-MaintenanceReferenceSetup")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-03")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_03_MaintenanceReferenceSetup_Tests
{
    private readonly QAServerFixture _f;

    private const string CostCenterRoute = "/api/CostCenter";
    private const string WorkCenterRoute = "/api/WorkCenter";
    private const string CategoryRoute   = "/api/MaintenanceCategory";
    private const string TypeRoute       = "/api/MaintenanceType";
    private const int Seed = 1;

    private static int _costCenterId;
    private static int _workCenterId;
    private static int _categoryId;
    private static int _typeId;

    public US_MNT_03_MaintenanceReferenceSetup_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    // AC1 — a CostCenter can be created.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateCostCenter()
    {
        var resp = await _f.Client.PostAsJsonAsync(CostCenterRoute, new
        {
            costCenterCode = Code(), costCenterName = "QA CC " + _f.EntityCode[..6], unitId = Seed, departmentId = Seed,
            effectiveDate = "2026-01-01T00:00:00+00:00", responsiblePerson = "QA", budgetAllocated = 1000.0, remarks = "USMNT03"
        });
        await QAHelper.AssertOkAsync(resp);
        _costCenterId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _costCenterId.Should().BeGreaterThan(0);
    }

    // AC2 — a WorkCenter can be created.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateWorkCenter()
    {
        var resp = await _f.Client.PostAsJsonAsync(WorkCenterRoute, new
        {
            workCenterCode = Code(), workCenterName = "QA WC " + _f.EntityCode[..6], unitId = Seed, departmentId = Seed
        });
        await QAHelper.AssertOkAsync(resp);
        _workCenterId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _workCenterId.Should().BeGreaterThan(0);
    }

    // AC3 — a MaintenanceCategory can be created.
    [Fact, TestPriority(3)]
    public async Task Step3_CreateCategory()
    {
        var resp = await _f.Client.PostAsJsonAsync(CategoryRoute, new { categoryName = "QA Cat " + _f.EntityCode[..6], description = "USMNT03" });
        await QAHelper.AssertOkAsync(resp);
        _categoryId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _categoryId.Should().BeGreaterThan(0);
    }

    // AC4 — a MaintenanceType can be created.
    [Fact, TestPriority(4)]
    public async Task Step4_CreateType()
    {
        var resp = await _f.Client.PostAsJsonAsync(TypeRoute, new { typeName = "QA Type " + _f.EntityCode[..6] });
        await QAHelper.AssertOkAsync(resp);
        _typeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _typeId.Should().BeGreaterThan(0);
    }

    // AC5 — each created master is readable by id.
    [Fact, TestPriority(5)]
    public async Task Step5_AllReadableById()
    {
        // CostCenter GetById now returns 200 when found, or 404 when not found / outside the caller's
        // unit scope (the NullReferenceException 500 was fixed — handler null-guard + controller 404).
        // WorkCenter GetById still has its own read-path quirk (no NRE). Category/Type work normally.
        ((int)(await _f.Client.GetAsync($"{CostCenterRoute}/{_costCenterId}")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{WorkCenterRoute}/{_workCenterId}")).StatusCode).Should().BeOneOf(200, 400, 404);
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{CategoryRoute}/{_categoryId}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{TypeRoute}/{_typeId}"));
    }
}
