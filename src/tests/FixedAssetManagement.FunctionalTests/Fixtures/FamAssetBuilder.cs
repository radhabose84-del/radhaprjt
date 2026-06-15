using System.Net.Http.Json;

namespace FixedAssetManagement.FunctionalTests.Fixtures;

// Shared helper that builds a fully-onboarded asset (classification hierarchy + location +
// UOM + AssetMasterGeneral with embedded location) and returns the new asset id. Used by the
// transaction-flow stories (coverage / disposal / etc.) that need a real asset to act on.
//
// Live-reconciled: AssetMasterGeneral requires CompanyId/Group/Category/SubCategory/Quantity/
// UOMId > 0 + a full AssetLocation block; CompanyId/UnitId are best-effort = 1 (testsales
// JWT carries 0). All names are run-unique (EntityCode-suffixed) so it is re-runnable.
public static class FamAssetBuilder
{
    private const int Seed = 1;

    public static async Task<int> BuildAssetAsync(HttpClient client, string entityCode)
    {
        string s = entityCode[..6];
        string Code(string p) => p + s;

        var groupId = await CreateIdAsync(client, "/api/AssetGroup",
            new { code = entityCode[..10], groupName = "QA AG " + s, groupPercentage = 10.0 });
        var categoryId = await CreateIdAsync(client, "/api/AssetCategories",
            new { categoryName = "QA Cat " + s, description = "FAM", assetGroupId = groupId });
        var subCategoryId = await CreateIdAsync(client, "/api/AssetSubCategories",
            new { subCategoryName = "QA SCat " + s, description = "FAM", assetCategoriesId = categoryId });
        var locationId = await CreateIdAsync(client, "/api/Location",
            new { code = Code("L"), locationName = "QA Loc " + s, description = "FAM", sortOrder = 1, unitId = Seed, departmentId = Seed });
        var subLocationId = await CreateIdAsync(client, "/api/SubLocation",
            new { code = Code("SL"), subLocationName = "QA SubLoc " + s, description = "FAM", unitId = Seed, departmentId = Seed, locationId });
        var uomId = await CreateIdAsync(client, "/api/fam/uom",
            new { code = Code("U"), uomName = "QA U " + s, sortOrder = 1, uomTypeId = Seed });

        var resp = await client.PostAsJsonAsync("/api/AssetMasterGeneral", new
        {
            assetMaster = new
            {
                companyId = Seed,
                unitId = Seed,
                assetName = "QA Asset " + s,
                assetGroupId = groupId,
                assetCategoryId = categoryId,
                assetSubCategoryId = subCategoryId,
                quantity = 1,
                uOMId = uomId,
                assetDescription = "QA built asset",
                assetLocation = new
                {
                    unitId = Seed,
                    departmentId = Seed,
                    locationId,
                    subLocationId,
                    custodianId = Seed,
                    userId = Seed
                }
            }
        });
        ((int)resp.StatusCode).Should().BeOneOf(200, 201); // AssetMasterGeneral create returns 201
        var assetId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        assetId.Should().BeGreaterThan(0);
        return assetId;
    }

    // POSTs a master create that returns 200/201 and extracts the new id (bare int or { id }).
    private static async Task<int> CreateIdAsync(HttpClient client, string route, object body)
    {
        var resp = await client.PostAsJsonAsync(route, body);
        ((int)resp.StatusCode).Should().BeOneOf(new[] { 200, 201 }, await resp.Content.ReadAsStringAsync());
        return (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
    }
}
