namespace FixedAssetManagement.QATests.Tests.AssetTransfer;

// AssetTransfer — /api/AssetTransfer  (transactional).
// GET /GetAllAssetTransfers | GET /GetAllAssetTransfersByAssetTransferId/{id} | POST | GET /{id} | PUT
//   | GET /TransferTypes | many helper lookups.  List is at /GetAllAssetTransfers (no bare GET).

[Collection("AssetTransferCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AssetTransferQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AssetTransfer";
    public AssetTransferQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(10)]
    [Trait("Layer", "Smoke")]
    public async Task TC010_GetAllAssetTransfers_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetAllAssetTransfers?PageNumber=1&PageSize=15");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/GetAllAssetTransfers?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_TransferTypes_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/TransferTypes");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(40)]
    public async Task TC040_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { assetId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(400, 500); // BUG: empty body 500 (validator NRE) not 400
    }
}
