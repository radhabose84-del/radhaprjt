namespace SalesManagement.QATests.Tests.MovementTypeConfig;

// ─────────────────────────────────────────────────────────────────────────────
// MovementTypeConfig — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/MovementTypeConfig            { movementCode, movementDescription, movementCategoryId,
//                                                fromStockTypeId, toStockTypeId, quantityUpdateFlag,
//                                                valueUpdateFlag, accountModifier?, batchRequiredFlag,
//                                                negativeStockAllowed }
//   PUT    /api/MovementTypeConfig            { id, movementDescription, movementCategoryId, fromStockTypeId,
//                                                toStockTypeId, quantityUpdateFlag, valueUpdateFlag,
//                                                accountModifier?, batchRequiredFlag, negativeStockAllowed,
//                                                isActive }                       (movementCode immutable)
//   DELETE /api/MovementTypeConfig?id={id}    (id bound from QUERY, not route)
//   GET    /api/MovementTypeConfig?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/MovementTypeConfig/{id}       (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/MovementTypeConfig/by-name?term=
//
// Key facts that shaped assertions:
//   • MovementCode is alphanumeric (^[A-Za-z0-9]+$), MAX LENGTH 4 → happy code ≤4 chars; 5-char triggers maxlength.
//   • MovementDescription is REQUIRED on Create (NotEmpty).
//   • movementCategoryId / fromStockTypeId / toStockTypeId are all same-module MiscMaster FKs
//       validated via MiscMasterExistsAsync → resolved at runtime from /api/sales/MiscMaster.
//   • Cross-field rule: fromStockTypeId MUST differ from toStockTypeId → "different" message.
//       Two DISTINCT MiscMaster ids are required for the happy path; if fewer than 2 exist the
//       create-happy chain is documented to fail at reconciliation (see ResolveTwoMiscMasterIdsAsync).
//   • AccountModifier becomes REQUIRED when valueUpdateFlag = true → happy path keeps valueUpdateFlag=false.
//   • AlreadyExists checks movementCode uniqueness.
//   • Delete validator: NotEmpty (id!=0) → NotFound (must exist).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("MovementTypeConfigCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MovementTypeConfigQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/MovementTypeConfig";
    private const string MiscMasterRoute = "/api/sales/MiscMaster";

    private const string TestDescription = "QA Test Movement Type Config";

    // The run-unique alphanumeric code (<= 4 chars) captured at create; reused by duplicate/immutability tests.
    private static string _createdCode = string.Empty;
    // Two distinct same-module MiscMaster FK ids resolved at create time.
    private static int _fromStockTypeId;
    private static int _toStockTypeId;
    private static int _categoryId;

    public MovementTypeConfigQATests(QAServerFixture fixture) => _f = fixture;

    // MovementCode max length is 4 — slice run-unique code to 4 chars.
    private string NewCode() => _f.EntityCode[..4];

    // Resolve two DISTINCT MiscMaster ids (and a category id) for the FK fields. The clone has no
    // guaranteed seed ids, so we read a page and pick distinct rows. Falls back to (1,2) when the
    // endpoint is empty/unreachable — live reconciliation will surface a clear failure if invalid.
    private async Task ResolveTwoMiscMasterIdsAsync()
    {
        var ids = new List<int>();
        var resp = await _f.Client.GetAsync($"{MiscMasterRoute}?PageNumber=1&PageSize=10");
        if (resp.IsSuccessStatusCode)
        {
            using var doc = await QAHelper.ParseAsync(resp);
            if (doc.RootElement.TryGetProperty("data", out var data) &&
                data.ValueKind == JsonValueKind.Array)
            {
                foreach (var row in data.EnumerateArray())
                {
                    if (row.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number)
                        ids.Add(idEl.GetInt32());
                }
            }
        }

        _fromStockTypeId = ids.Count >= 1 ? ids[0] : 1;
        _toStockTypeId = ids.Count >= 2 ? ids[1] : 2;   // must differ from _fromStockTypeId
        _categoryId = ids.Count >= 1 ? ids[0] : 1;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdCode = NewCode();
        await ResolveTwoMiscMasterIdsAsync();

        // NOTE: requires two DISTINCT valid MiscMaster ids. If the clone has < 2, this fails at
        // reconciliation (the from==to cross-field rule blocks identical ids).
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            movementCode = _createdCode,
            movementDescription = TestDescription,
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,   // keep false so accountModifier stays optional
            accountModifier = (string?)null,
            batchRequiredFlag = false,
            negativeStockAllowed = false
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
            movementCode = "NA01",
            movementDescription = TestDescription,
            movementCategoryId = 1,
            fromStockTypeId = 1,
            toStockTypeId = 2,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        await ResolveTwoMiscMasterIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            movementCode = "",
            movementDescription = TestDescription,
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DescriptionEmpty_Returns400()
    {
        await ResolveTwoMiscMasterIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            movementCode = NewCode(),
            movementDescription = "",
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CodeTooLong_Returns400()
    {
        // MovementCode max length is 4 → 5 chars triggers maxlength.
        await ResolveTwoMiscMasterIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            movementCode = "ABCDE", // 5 chars > 4
            movementDescription = TestDescription,
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_CodeWithSpace_Returns400()
    {
        await ResolveTwoMiscMasterIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            movementCode = "A B",
            movementDescription = TestDescription,
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_FromEqualsTo_Returns400()
    {
        // Cross-field rule: fromStockTypeId must differ from toStockTypeId.
        await ResolveTwoMiscMasterIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            movementCode = NewCode(),
            movementDescription = TestDescription,
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _fromStockTypeId, // identical → blocked
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "different");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_InvalidFromStockType_Returns400()
    {
        // MiscMasterExistsAsync false → FK validation fails.
        await ResolveTwoMiscMasterIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            movementCode = NewCode(),
            movementDescription = TestDescription,
            movementCategoryId = _categoryId,
            fromStockTypeId = 999999,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_DuplicateCode_Returns400()
    {
        // Same code as TC001 → AlreadyExists fails.
        await ResolveTwoMiscMasterIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            movementCode = _createdCode,
            movementDescription = TestDescription,
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_EmptyBody_Returns400()
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
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
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
    public async Task TC022_GetAll_SearchByCreatedCode_Returns200_WithData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdCode}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200_WithCorrectCode()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("movementCode").GetString()
            .Should().Be(_createdCode);
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
    public async Task TC040_AutoComplete_WithTerm_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (MovementCode is immutable — not in the update command)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            movementDescription = "QA Updated Movement Config",
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            accountModifier = (string?)null,
            batchRequiredFlag = false,
            negativeStockAllowed = false,
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
            movementDescription = "QA Updated Movement Config",
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_DescriptionEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            movementDescription = "",
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_FromEqualsTo_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            movementDescription = "QA Updated Movement Config",
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _fromStockTypeId, // identical → blocked
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "different");
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            movementDescription = "QA Updated Movement Config",
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false,
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            movementDescription = "QA Updated Movement Config",
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            movementDescription = "QA Updated Movement Config",
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            movementDescription = "QA Updated Movement Config",
            movementCategoryId = _categoryId,
            fromStockTypeId = _fromStockTypeId,
            toStockTypeId = _toStockTypeId,
            quantityUpdateFlag = true,
            valueUpdateFlag = false,
            batchRequiredFlag = false,
            negativeStockAllowed = false,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    [Fact, TestPriority(57)]
    public async Task TC057_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(58)]
    public async Task TC058_Verify_CodeIsImmutable_GetByIdShowsOriginalCode()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("movementCode").GetString()
            .Should().Be(_createdCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from QUERY: ?id={id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=0");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

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

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(95)]
    public async Task TC095_VerifySoftDelete_GetByIdReturns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
