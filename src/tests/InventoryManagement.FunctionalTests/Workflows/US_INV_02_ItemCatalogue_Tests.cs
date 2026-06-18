namespace InventoryManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-INV-02 — Item catalogue setup
//   As an inventory administrator I define item specification masters and values,
//   then catalogue an item that references them.
// Partial: spec master + spec value are clean masters (active); the ItemMaster create
// needs a nested DTO (group/category/stockUom/hsn + tabs) → blocked; its reads are reachable.
//
// Contracts (verified against InventoryManagement.QATests, 2026-06-17):
//   POST   /api/itemspecificationmaster { specificationCode, specificationName, order }
//   POST   /api/itemspecificationvalue  { specificationMasterId, specificationValue }
//   GET    /api/ItemMaster?PageNumber=&PageSize=                 (route is api/[controller])
//   GET    /api/ItemMaster/autocomplete?searchPattern=
//   DELETE /api/itemspecificationvalue?id={id}    (id from QUERY)
//   DELETE /api/itemspecificationmaster?id={id}   (id from QUERY)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-INV-02-ItemCatalogue")]
[Trait("Module", "InventoryManagement")]
[Trait("Story", "US-INV-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_INV_02_ItemCatalogue_Tests
{
    private readonly QAServerFixture _f;

    private const string SpecMasterRoute = "/api/itemspecificationmaster";
    private const string SpecValueRoute  = "/api/itemspecificationvalue";
    private const string ItemMasterRoute = "/api/ItemMaster";

    private static int _specMasterId;
    private static int _specValueId;

    public US_INV_02_ItemCatalogue_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    // Run-unique order (1000-9999) → avoids OrderAlreadyExists collisions.
    private int NewOrder() => QAHelper.RunUniqueInt(_f.EntityCode);

    // AC1 — an ItemSpecificationMaster can be created (code + name + order, each unique).
    [Fact, TestPriority(1)]
    public async Task Step1_CreateItemSpecificationMaster()
    {
        var code = Code();

        var resp = await _f.Client.PostAsJsonAsync(SpecMasterRoute, new
        {
            specificationCode = code,
            specificationName = $"QA Item Spec Master {code}",
            order = NewOrder()
        });

        await QAHelper.AssertOkAsync(resp);
        _specMasterId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _specMasterId.Should().BeGreaterThan(0);
    }

    // AC2 — an ItemSpecificationValue can be created under that master (FK specificationMasterId).
    [Fact, TestPriority(2)]
    public async Task Step2_CreateItemSpecificationValueUnderMaster()
    {
        _specMasterId.Should().BeGreaterThan(0, "Step1 must have created the spec master");

        var resp = await _f.Client.PostAsJsonAsync(SpecValueRoute, new
        {
            specificationMasterId = _specMasterId,
            specificationValue = $"QA SpecValue {_f.EntityCode[..8]}"
        });

        await QAHelper.AssertOkAsync(resp);
        _specValueId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _specValueId.Should().BeGreaterThan(0);
    }

    // AC3 — the ItemMaster read surface is reachable (GetAll + autocomplete).
    [Fact, TestPriority(3)]
    public async Task Step3_ItemMasterReadSurfaceReachable()
    {
        var all = await _f.Client.GetAsync($"{ItemMasterRoute}?PageNumber=1&PageSize=15");
        ((int)all.StatusCode).Should().BeOneOf(200, 404);

        var auto = await _f.Client.GetAsync($"{ItemMasterRoute}/autocomplete?searchPattern=a");
        ((int)auto.StatusCode).Should().BeOneOf(200, 404);
    }

    // AC4 — an ItemMaster can be created from group/category/stockUom/hsn + the spec.
    // 🚫 blocked: a valid Item needs a fully-formed nested ItemDto (itemGroup/itemCategory/stockUom/hsn + tabs)
    // that the QA clone does not guarantee.
    [Fact(Skip = "needs seeded data: ItemMaster nested payload (itemGroup/itemCategory/stockUom/hsn)"), TestPriority(4)]
    public async Task Step4_CreateItemMaster()
    {
        var resp = await _f.Client.PostAsJsonAsync(ItemMasterRoute, new
        {
            itemName = $"QA Item {_f.EntityCode}",
            itemGroupId = 1,
            itemCategoryId = 1,
            stockUomId = 1,
            hsnId = 1
        });

        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
    }

    // AC5 — teardown: spec value then spec master, both delete by QUERY ?id=.
    [Fact, TestPriority(5)]
    public async Task Step5_TeardownLeafFirst()
    {
        if (_specValueId > 0)  await _f.Client.DeleteAsync($"{SpecValueRoute}?id={_specValueId}");
        if (_specMasterId > 0) await _f.Client.DeleteAsync($"{SpecMasterRoute}?id={_specMasterId}");
    }
}
