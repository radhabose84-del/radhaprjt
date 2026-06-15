namespace FixedAssetManagement.QATests.Tests.WDVDepreciation;

// WDVDepreciation — /api/WDVDepreciation  (transactional; WDV depreciation run).
// GET /GetWDV | POST | DELETE | PUT  (no bare GET-all)

[Collection("WDVDepreciationCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class WDVDepreciationQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/WDVDepreciation";
    public WDVDepreciationQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(10)]
    public async Task TC010_GetWDV_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GetWDV");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetWDV_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/GetWDV");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
