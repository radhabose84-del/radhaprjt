// ─────────────────────────────────────────────────────────────────────────────
// UserSignature — Live-server QA tests
//
// Route:    /api/UserSignature  (controller UserSignatureController)
//   • GET    /api/UserSignature                 → GetAll (paged; 404 when empty)
//   • GET    /api/UserSignature/{id}            → GetById (NO null-guard → 200)
//   • GET    /api/UserSignature/by-user/{userId}→ GetByUserId (always 200 wrapper)
//   • POST   /api/UserSignature                 → multipart/form-data (UserId + File)
//   • PUT    /api/UserSignature/{id}            → multipart/form-data (File + IsActive)
//   • DELETE /api/UserSignature/{id}            → no pre-guard → 200
//
// BLOCKED (create/update-happy): POST/PUT are [Consumes("multipart/form-data")] file
//   uploads and require a real userId + an image file. Skipped with reason.
//
// ALWAYS-ACTIVE: GetAll (Smoke, tolerant 200/404), GetAll no-auth 401, GetById
//   nonexistent reachability, by-user/{userId} reachability, DELETE no-auth 401.
//
// Conventions: matches existing UserManagement.QATests.
// ─────────────────────────────────────────────────────────────────────────────

namespace UserManagement.QATests.Tests.UserSignature;

[Collection("UserSignatureCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class UserSignatureQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/UserSignature";

    public UserSignatureQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — GET ALL (primary GET — Smoke)
    // Controller returns 404 when no signatures exist; 200 when present. Tolerant.
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_GetAll_Page2PageSize5_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET BY ID  (no null-guard → 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(4)]
    public async Task TC004_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_GetById_NonExistent_Reachable()
    {
        // No null-guard → typically 200+null; tolerate 200/400/404.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY USER
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(6)]
    public async Task TC006_ByUser_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-user/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_ByUser_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-user/1");
        await QAHelper.Assert401Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — CREATE / UPDATE  (multipart/form-data file upload — BLOCKED)
    // ─────────────────────────────────────────────────────────────────────────

    // BLOCKED: multipart/form-data file upload; needs a real userId + image file.
    [Fact(Skip = "needs seeded data: UserSignature Create is multipart/form-data and requires a real userId + signature image file."), TestPriority(8)]
    public async Task TC008_Create_HappyPath_Returns200()
    {
        using var content = new MultipartFormDataContent
        {
            { new StringContent("1"), "UserId" },
            { new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 }), "File", "sig.png" }
        };
        var resp = await _f.Client.PostAsync(BaseRoute, content);
        await QAHelper.AssertOkAsync(resp);
    }

    // BLOCKED: multipart/form-data file upload; needs a created signature id + image file.
    [Fact(Skip = "needs seeded data: UserSignature Update is multipart/form-data and requires a created signature id + image file."), TestPriority(9)]
    public async Task TC009_Update_HappyPath_Returns200()
    {
        using var content = new MultipartFormDataContent
        {
            { new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 }), "File", "sig.png" },
            { new StringContent("1"), "IsActive" }
        };
        var resp = await _f.Client.PutAsync($"{BaseRoute}/{_f.CreatedId}", content);
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — DELETE  (no pre-guard → 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(10)]
    public async Task TC010_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/1");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Delete_NonExistent_Reachable()
    {
        // No pre-query guard → handler deletes 0 rows; controller returns 200. Tolerant.
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
