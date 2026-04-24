using BackgroundService.Infrastructure.Repositories.Notification;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BackgroundService.IntegrationTests.Repositories.NotificationTablePreset
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationTablePresetRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationTablePresetRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private NotificationTablePresetRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task EnsureSchemaAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Notification')
                    EXEC('CREATE SCHEMA [Notification]');");
            await conn.ExecuteAsync(@"
                IF OBJECT_ID('Notification.TablePresets') IS NULL
                BEGIN
                    CREATE TABLE Notification.TablePresets(
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        TemplateId int NOT NULL,
                        ColumnsJson nvarchar(max) NULL,
                        IsActive bit NOT NULL DEFAULT 1,
                        [Version] int NULL);
                END");
        }

        private async Task ClearAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                IF OBJECT_ID('Notification.TablePresets') IS NOT NULL
                    DELETE FROM Notification.TablePresets;");
        }

        private async Task SeedPresetAsync(int templateId, string json, bool isActive = true, int? version = null)
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                INSERT INTO Notification.TablePresets (TemplateId, ColumnsJson, IsActive, [Version])
                VALUES (@TemplateId, @ColumnsJson, @IsActive, @Version);",
                new { TemplateId = templateId, ColumnsJson = json, IsActive = isActive, Version = version });
        }

        [Fact]
        public async Task GetColumnsJsonByTemplateIdAsync_Returns_Null_When_Empty()
        {
            await EnsureSchemaAsync();
            await ClearAsync();

            var result = await CreateRepo().GetColumnsJsonByTemplateIdAsync(42, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetColumnsJsonByTemplateIdAsync_Returns_Matching_Row()
        {
            await EnsureSchemaAsync();
            await ClearAsync();
            await SeedPresetAsync(templateId: 7, json: "[\"col1\",\"col2\"]");

            var result = await CreateRepo().GetColumnsJsonByTemplateIdAsync(7, CancellationToken.None);

            result.Should().Be("[\"col1\",\"col2\"]");
        }

        [Fact]
        public async Task GetColumnsJsonByTemplateIdAsync_Filters_By_TemplateId()
        {
            await EnsureSchemaAsync();
            await ClearAsync();
            await SeedPresetAsync(templateId: 10, json: "A");
            await SeedPresetAsync(templateId: 20, json: "B");

            var result = await CreateRepo().GetColumnsJsonByTemplateIdAsync(10, CancellationToken.None);

            result.Should().Be("A");
        }

        [Fact]
        public async Task GetColumnsJsonByTemplateIdAsync_Excludes_Inactive()
        {
            await EnsureSchemaAsync();
            await ClearAsync();
            await SeedPresetAsync(templateId: 30, json: "inactive", isActive: false);

            var result = await CreateRepo().GetColumnsJsonByTemplateIdAsync(30, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetColumnsJsonByTemplateIdAsync_Returns_Highest_Version()
        {
            await EnsureSchemaAsync();
            await ClearAsync();
            await SeedPresetAsync(templateId: 50, json: "v1", version: 1);
            await SeedPresetAsync(templateId: 50, json: "v3", version: 3);
            await SeedPresetAsync(templateId: 50, json: "v2", version: 2);

            var result = await CreateRepo().GetColumnsJsonByTemplateIdAsync(50, CancellationToken.None);

            result.Should().Be("v3");
        }

        [Fact]
        public async Task GetColumnsJsonByTemplateIdAsync_Treats_NullVersion_As_Zero_In_Order()
        {
            await EnsureSchemaAsync();
            await ClearAsync();
            await SeedPresetAsync(templateId: 60, json: "nullVer", version: null);
            await SeedPresetAsync(templateId: 60, json: "ver5",    version: 5);

            var result = await CreateRepo().GetColumnsJsonByTemplateIdAsync(60, CancellationToken.None);

            result.Should().Be("ver5");
        }

        [Fact]
        public async Task GetColumnsJsonByTemplateIdAsync_Returns_Only_Active_Highest_Version()
        {
            await EnsureSchemaAsync();
            await ClearAsync();
            await SeedPresetAsync(templateId: 70, json: "v9_inactive", version: 9, isActive: false);
            await SeedPresetAsync(templateId: 70, json: "v3_active",   version: 3, isActive: true);

            var result = await CreateRepo().GetColumnsJsonByTemplateIdAsync(70, CancellationToken.None);

            result.Should().Be("v3_active");
        }
    }
}
