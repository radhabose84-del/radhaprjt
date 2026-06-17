namespace SalesManagement.QATests.Tests.CommissionSplit;

// ─────────────────────────────────────────────────────────────────────────────
// CommissionSplit — live-server QA suite (master + nested details CRUD + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/CommissionSplit
//          {
//            splitName,
//            details:[{ roleId, shareTypeId, shareValue }]   // EXACTLY 2 rows
//          }
//   PUT    /api/CommissionSplit   { id, splitName, isActive, details:[…] }
//   DELETE /api/CommissionSplit?id={id}   (id bound from QUERY — controller action param `int id`)
//   GET    /api/CommissionSplit?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/CommissionSplit/{id}      (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/CommissionSplit/by-name?term=
//
// Nested-detail business rules (from CreateCommissionSplitCommandValidator):
//   • splitName required + unique (AlreadyExistsAsync) + MaxLength.
//   • Details must have EXACTLY 2 rows ("Exactly two split configuration rows are required …").
//   • No duplicate roleIds across rows; all rows must use the SAME shareTypeId.
//   • roleId & shareTypeId are same-module MiscMaster FKs (MiscMasterExistsAsync); shareValue > 0.
//   • If the shared shareTypeId's MiscMaster code == "PERCENTAGE" → sum(shareValue) must equal 100.
//
// FK / seeding note:
//   A valid create needs TWO DISTINCT MiscMaster ids for the roleIds (FirstIdAsync gives only one),
//   plus a shareTypeId, and — if that share type is PERCENTAGE — the values must sum to 100. The QA
//   clone has no guaranteed seed for two distinct MiscMaster role rows, so the create-happy step and
//   every test that depends on a created id are wrapped in [Fact(Skip="needs seeded data: …")].
//   The reachability/negative/GetAll(Smoke) tests stay ACTIVE.
//
// Delete action binds `int id` from query (?id=). GetById has no null guard → 200+data:null.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("CommissionSplitCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CommissionSplitQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/CommissionSplit";
    private const string MiscMasterRoute = "/api/sales/MiscMaster";

    private static string _createdName = string.Empty;

    public CommissionSplitQATests(QAServerFixture fixture) => _f = fixture;

    private string NewName() => $"QA Split {_f.EntityCode[..8]}";

    // Resolves up to two distinct MiscMaster ids for the two role rows. Returns (role1, role2, shareType).
    // Falls back to 1/2 if the clone has fewer than two rows (create step is Skipped anyway).
    private async Task<(int role1, int role2, int shareType)> ResolveMiscIdsAsync()
    {
        var resp = await _f.Client.GetAsync($"{MiscMasterRoute}?PageNumber=1&PageSize=5");
        var ids = new List<int>();
        if (resp.IsSuccessStatusCode)
        {
            using var doc = await QAHelper.ParseAsync(resp);
            if (doc.RootElement.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in data.EnumerateArray())
                    if (el.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number)
                        ids.Add(idProp.GetInt32());
            }
        }
        var r1 = ids.Count > 0 ? ids[0] : 1;
        var r2 = ids.Count > 1 ? ids[1] : 2;
        var st = ids.Count > 2 ? ids[2] : r1;
        return (r1, r2, st);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: two distinct MiscMaster role ids + a non-PERCENTAGE share type (or values summing to 100) — not guaranteed in the QA clone"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdName = NewName();
        var (role1, role2, shareType) = await ResolveMiscIdsAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            splitName = _createdName,
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 60m },
                new { roleId = role2, shareTypeId = shareType, shareValue = 40m }
            }
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
            splitName = "QA NoAuth Split",
            details = new[]
            {
                new { roleId = 1, shareTypeId = 1, shareValue = 50m },
                new { roleId = 2, shareTypeId = 1, shareValue = 50m }
            }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NameEmpty_Returns400()
    {
        var (role1, role2, shareType) = await ResolveMiscIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            splitName = "",
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 50m },
                new { roleId = role2, shareTypeId = shareType, shareValue = 50m }
            }
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_NameTooLong_Returns400()
    {
        var (role1, role2, shareType) = await ResolveMiscIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            splitName = new string('A', 201),
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 50m },
                new { roleId = role2, shareTypeId = shareType, shareValue = 50m }
            }
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_MissingDetails_Returns400()
    {
        // null details → "Exactly two split configuration rows are required …"
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            splitName = NewName()
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_WrongDetailCount_OneRow_Returns400()
    {
        // Exactly 2 rows required → one row fails.
        var (role1, _, shareType) = await ResolveMiscIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            splitName = NewName(),
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 100m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_DuplicateRoles_Returns400()
    {
        // Same roleId in both rows → "Duplicate roles are not allowed."
        var (role1, _, shareType) = await ResolveMiscIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            splitName = NewName(),
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 50m },
                new { roleId = role1, shareTypeId = shareType, shareValue = 50m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_InvalidMiscMasterFk_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            splitName = NewName(),
            details = new[]
            {
                new { roleId = 999999, shareTypeId = 999999, shareValue = 50m },
                new { roleId = 888888, shareTypeId = 999999, shareValue = 50m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact(Skip = "needs seeded data: a successfully created CommissionSplit (TC001 is skipped)"), TestPriority(10)]
    public async Task TC010_Create_DuplicateName_Returns400()
    {
        var (role1, role2, shareType) = await ResolveMiscIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            splitName = _createdName,
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 60m },
                new { roleId = role2, shareTypeId = shareType, shareValue = 40m }
            }
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
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

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (no null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a successfully created CommissionSplit (TC001 is skipped)"), TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
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
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a successfully created CommissionSplit (TC001 is skipped)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var (role1, role2, shareType) = await ResolveMiscIdsAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            splitName = _createdName + " Upd",
            isActive = 1,
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 70m },
                new { roleId = role2, shareTypeId = shareType, shareValue = 30m }
            }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 1,
            splitName = "QA Upd",
            isActive = 1,
            details = new[]
            {
                new { roleId = 1, shareTypeId = 1, shareValue = 50m },
                new { roleId = 2, shareTypeId = 1, shareValue = 50m }
            }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NonExistentId_Returns400_NotFound()
    {
        var (role1, role2, shareType) = await ResolveMiscIdsAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            splitName = NewName(),
            isActive = 1,
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 50m },
                new { roleId = role2, shareTypeId = shareType, shareValue = 50m }
            }
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact(Skip = "needs seeded data: a successfully created CommissionSplit (TC001 is skipped)"), TestPriority(54)]
    public async Task TC054_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var (role1, role2, shareType) = await ResolveMiscIdsAsync();

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            splitName = _createdName + " Upd",
            isActive = 0,
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 70m },
                new { roleId = role2, shareTypeId = shareType, shareValue = 30m }
            }
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            splitName = _createdName + " Upd",
            isActive = 1,
            details = new[]
            {
                new { roleId = role1, shareTypeId = shareType, shareValue = 70m },
                new { roleId = role2, shareTypeId = shareType, shareValue = 30m }
            }
        });
        await QAHelper.AssertOkAsync(reactivate);
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
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: a successfully created CommissionSplit (TC001 is skipped)"), TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact(Skip = "needs seeded data: a successfully created+deleted CommissionSplit (TC001/TC093 are skipped)"), TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }
}
