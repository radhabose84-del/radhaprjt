namespace InventoryManagement.QATests.Tests.ItemSpecificationMaster;

// ─────────────────────────────────────────────────────────────────────────────
// ItemSpecificationMaster — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-17 — ItemSpecificationMasterController.cs):
//   POST   /api/itemspecificationmaster        { specificationCode, specificationName, order }
//   PUT    /api/itemspecificationmaster        { id, specificationName, order, isActive }
//   DELETE /api/itemspecificationmaster?id={id}  (id bound from QUERY — action param `int id`)
//   GET    /api/itemspecificationmaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/itemspecificationmaster/{id}   (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/itemspecificationmaster/by-name?term=
//
// Key facts that shaped assertions (CreateItemSpecificationMasterCommandValidator):
//   • specificationCode: required, alphanumeric (^[A-Za-z0-9]+$), MaxLength(20), unique (AlreadyExists). Immutable on update.
//   • specificationName: required, MaxLength(100), unique (NameAlreadyExists).
//   • order: int, GreaterThan(0), unique (OrderAlreadyExists).
//   • Code/Name/Order are all unique → run-unique values are derived from EntityCode to stay re-runnable.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ItemSpecificationMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ItemSpecificationMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/itemspecificationmaster";

    private const string TestName = "QA Item Spec Master";

    private static string _createdCode = string.Empty;
    private static int _createdOrder;

    public ItemSpecificationMasterQATests(QAServerFixture fixture) => _f = fixture;

    // Run-unique alphanumeric code (Q + reversed-tick digits), sliced to 10 chars.
    private string NewCode() => _f.EntityCode[..10];

    // Run-unique order (1000-9999) derived from EntityCode → avoids OrderAlreadyExists collisions.
    private int NewOrder() => QAHelper.RunUniqueInt(_f.EntityCode);

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdCode = NewCode();
        _createdOrder = NewOrder();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = _createdCode,
            specificationName = TestName + " " + _createdCode,
            order = _createdOrder
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = "NOAUTH01",
            specificationName = "No Auth Spec",
            order = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = "",
            specificationName = TestName,
            order = NewOrder()
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = NewCode(),
            specificationName = "",
            order = NewOrder()
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_OrderZero_Returns400()
    {
        // order GreaterThan(0) → 0 fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = NewCode(),
            specificationName = TestName,
            order = 0
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_CodeTooLong_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = new string('A', 101), // exceeds code max (20)
            specificationName = TestName,
            order = NewOrder()
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_NameTooLong_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = NewCode(),
            specificationName = new string('A', 101), // exceeds name max (100)
            order = NewOrder()
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_CodeWithSpace_Returns400()
    {
        // Alphanumeric pattern ^[A-Za-z0-9]+$ — spaces are not allowed.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = "QA SPEC",
            specificationName = TestName,
            order = NewOrder()
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_CodeWithSpecialChar_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = "QA@SPEC",
            specificationName = TestName,
            order = NewOrder()
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_DuplicateCode_Returns400()
    {
        // Same code as TC001 → AlreadyExists fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            specificationCode = _createdCode,
            specificationName = TestName + " dup",
            order = NewOrder() + 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_SearchByCreatedCode_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdCode}");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (Code is immutable — not in the update command)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            specificationName = "QA Updated Spec " + _createdCode,
            order = _createdOrder,
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            specificationName = "QA Updated Spec",
            order = _createdOrder,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            specificationName = "",
            order = _createdOrder,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            specificationName = "QA Updated Spec " + _createdCode,
            order = _createdOrder,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            specificationName = "QA Updated Spec " + _createdCode,
            order = _createdOrder,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from QUERY: ?id={id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(95)]
    public async Task TC095_VerifySoftDelete_GetByIdReturns200_WithNullData()
    {
        // After soft delete, GetByIdAsync filters IsDeleted=0 → null → 200 + data:null.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
