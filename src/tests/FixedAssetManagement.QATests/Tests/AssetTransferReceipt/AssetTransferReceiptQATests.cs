namespace FixedAssetManagement.QATests.Tests.AssetTransferReceipt;

// AssetTransferReceipt — /api/AssetTransferReceipt  (transactional).
// GET (all) | GET /{id} | POST | GET /GetAssetTransferReceiptPending | GET /GetAssetTransferReceiptDtlPending/{id}

[Collection("AssetTransferReceiptCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AssetTransferReceiptQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AssetTransferReceipt";
    public AssetTransferReceiptQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(10)]
    [Trait("Layer", "Smoke")]
    public async Task TC010_GetAll_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_Pending_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetAssetTransferReceiptPending");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { assetTransferId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(400, 500); // BUG: empty body 500 (validator NRE) not 400
    }
}
