using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationDetail;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BackgroundService.IntegrationTests.Repositories.NotificationDetail
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationDetailRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationDetailRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private NotificationDetailRepository CreateRepo(int? readStatusMiscId = 1)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(s => s.GetUnitId()).Returns(1);
            var miscLookup = new Mock<IAppDataMiscMasterLookup>(MockBehavior.Loose);
            miscLookup.Setup(x => x.GetMiscMasterByNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(readStatusMiscId.HasValue ? new MiscMasterLookupDto { Id = readStatusMiscId.Value } : null);

            return new NotificationDetailRepository(
                conn,
                _fixture.CreateFreshDbContext(),
                ipMock.Object,
                miscLookup.Object);
        }

        private async Task<(int channelId, int statusId, int readStatusId)> EnsureMiscRowsAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var miscTypeId = await conn.ExecuteScalarAsync<int?>(
                "SELECT TOP 1 Id FROM AppData.MiscTypeMaster WHERE MiscTypeCode = 'NotifType'");
            if (miscTypeId == null)
            {
                miscTypeId = await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO AppData.MiscTypeMaster
                        (MiscTypeCode, Description, IsActive, IsDeleted, CreatedBy, CreatedByName, CreatedIP)
                    OUTPUT INSERTED.Id
                    VALUES
                        ('NotifType', 'Notification Type', 1, 0, 1, 'test-user', '127.0.0.1');");
            }

            async Task<int> EnsureMiscAsync(string code)
            {
                var existing = await conn.ExecuteScalarAsync<int?>(
                    "SELECT TOP 1 Id FROM AppData.MiscMaster WHERE Code = @Code AND MiscTypeId = @MTId",
                    new { Code = code, MTId = miscTypeId.Value });
                if (existing != null) return existing.Value;

                return await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO AppData.MiscMaster
                        (MiscTypeId, Code, Description, IsActive, IsDeleted, CreatedBy, CreatedByName, CreatedIP)
                    OUTPUT INSERTED.Id
                    VALUES
                        (@MTId, @Code, @Code, 1, 0, 1, 'test-user', '127.0.0.1');",
                    new { Code = code, MTId = miscTypeId.Value });
            }

            return (
                await EnsureMiscAsync("Channel"),
                await EnsureMiscAsync("Status"),
                await EnsureMiscAsync("ReadStatus"));
        }

        private async Task<int> SeedEventLogAsync()
        {
            var (channelId, statusId, readStatusId) = await EnsureMiscRowsAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new NotificationEventLog
            {
                NotificationLevelRuleId = null,
                UnitId = 1,
                ChannelId = channelId,
                NotificationStatusId = statusId,
                ReadStatusId = readStatusId,
                MessageText = "Hello",
                SendTo = "user1",
                ActionStatus = "Sent",
                Timestamp = DateTimeOffset.UtcNow
            };
            await ctx.NotificationEventLog.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var logs = await ctx.NotificationEventLog.ToListAsync();
            ctx.NotificationEventLog.RemoveRange(logs);
            await ctx.SaveChangesAsync();
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Return_One_And_Set_ReadStatusId_From_MiscLookup()
        {
            await ClearAsync();
            var logId = await SeedEventLogAsync();
            // Resolve an existing MiscMaster Id to satisfy the FK_NotificationEventLog_MiscMaster_ReadStatusId constraint
            var (_, _, readStatusMiscId) = await EnsureMiscRowsAsync();

            var result = await CreateRepo(readStatusMiscId: readStatusMiscId).UpdateAsync(logId, new NotificationEventLog());

            result.Should().Be(1);

            await using var ctx = _fixture.CreateFreshDbContext();
            var updated = await ctx.NotificationEventLog.AsNoTracking().FirstAsync(l => l.Id == logId);
            updated.ReadStatusId.Should().Be(readStatusMiscId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_MinusOne_When_Log_NotFound()
        {
            await ClearAsync();

            var result = await CreateRepo().UpdateAsync(999999, new NotificationEventLog());

            result.Should().Be(-1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_MiscLookup_Returns_Null()
        {
            // The repository requires the 'Read' MiscMaster entry to exist (FK to AppData.MiscMaster).
            // When the lookup returns null, the repo throws InvalidOperationException rather than
            // silently inserting an invalid FK value (Id = 0).
            await ClearAsync();
            var logId = await SeedEventLogAsync();

            Func<Task> act = async () => await CreateRepo(readStatusMiscId: null)
                .UpdateAsync(logId, new NotificationEventLog());

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*MiscMaster entry not found*");
        }
    }
}
