using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.CustomFields;
using UserManagement.Infrastructure.Repositories.MiscMaster;
using UserManagement.Infrastructure.Repositories.MiscTypeMaster;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.CustomFields
{
    [Collection("DatabaseCollection")]
    public sealed class CustomFieldCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CustomFieldCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApplicationDbContext CreateDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private CustomFieldCommand CreateRepository(ApplicationDbContext ctx)
            => new CustomFieldCommand(ctx);

        private async Task<(int MiscTypeId, int LabelTypeId, int DataTypeId)> EnsureMiscMasterAsync(ApplicationDbContext ctx)
        {
            // Ensure MiscTypeMaster
            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "CF_TYPE");
            if (miscType == null)
            {
                miscType = new UserManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "CF_TYPE",
                    Description = "Custom Field Type",
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            // Ensure LabelType MiscMaster
            var labelType = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "LBLTYPE" && m.MiscTypeId == miscType.Id);
            if (labelType == null)
            {
                labelType = new UserManagement.Domain.Entities.MiscMaster
                {
                    Code = "LBLTYPE",
                    Description = "Label Type",
                    MiscTypeId = miscType.Id,
                    SortOrder = 1,
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(labelType);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            // Ensure DataType MiscMaster
            var dataType = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "DATATYPE" && m.MiscTypeId == miscType.Id);
            if (dataType == null)
            {
                dataType = new UserManagement.Domain.Entities.MiscMaster
                {
                    Code = "DATATYPE",
                    Description = "Data Type",
                    MiscTypeId = miscType.Id,
                    SortOrder = 2,
                    IsActive = Enums.Status.Active,
                    IsDeleted = Enums.IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(dataType);
                await ctx.SaveChangesAsync();
                ctx.ChangeTracker.Clear();
            }

            return (miscType.Id, labelType.Id, dataType.Id);
        }

        private async Task ClearTestDataAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM AppData.CustomField WHERE LabelName LIKE 'TestCF%'");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var (_, labelTypeId, dataTypeId) = await EnsureMiscMasterAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = new CustomField
            {
                LabelName = "TestCF_Create",
                LabelTypeId = labelTypeId,
                DataTypeId = dataTypeId,
                Length = 100,
                IsRequired = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var newId = await repo.CreateAsync(entity);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var (_, labelTypeId, dataTypeId) = await EnsureMiscMasterAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = new CustomField
            {
                LabelName = "TestCF_Persist",
                LabelTypeId = labelTypeId,
                DataTypeId = dataTypeId,
                Length = 50,
                IsRequired = 0,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.CustomField.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.LabelName.Should().Be("TestCF_Persist");
            saved.Length.Should().Be(50);
            saved.IsDeleted.Should().Be(Enums.IsDelete.NotDeleted);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Soft_Delete_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync(ctx);
            var (_, labelTypeId, dataTypeId) = await EnsureMiscMasterAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = new CustomField
            {
                LabelName = "TestCF_Delete",
                LabelTypeId = labelTypeId,
                DataTypeId = dataTypeId,
                Length = 100,
                IsRequired = 1,
                IsActive = Enums.Status.Active,
                IsDeleted = Enums.IsDelete.NotDeleted
            };

            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var deleteModel = new CustomField { IsDeleted = Enums.IsDelete.Deleted };
            var result = await repo.DeleteAsync(newId, deleteModel);

            result.Should().BeTrue();

            ctx.ChangeTracker.Clear();
            var deleted = await ctx.CustomField.FirstOrDefaultAsync(x => x.Id == newId);
            deleted!.IsDeleted.Should().Be(Enums.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = CreateDbContext();

            var repo = CreateRepository(ctx);
            var result = await repo.DeleteAsync(99999, new CustomField { IsDeleted = Enums.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
