using Microsoft.EntityFrameworkCore;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.MiscMaster;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.Infrastructure.Repositories.QualityParameter;
using QCManagement.Infrastructure.Repositories.QualityTemplate;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.QualityTemplate
{
    [Collection("DatabaseCollection")]
    public sealed class QualityTemplateCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QualityTemplateCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private QualityTemplateCommandRepository CreateRepo(ApplicationDbContext ctx)
            => new QualityTemplateCommandRepository(ctx);

        private async Task ClearAllAsync() =>
            await _fixture.ClearTablesAsync(
                "QC.QualityTemplateParameter", "QC.QualityTemplate",
                "QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        private async Task<(int qpId, int inspMethodId)> SeedDependenciesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var groupType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_GROUP", Description = "Group", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_DATATYPE", Description = "Data Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_VALIDATION", Description = "Validation Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var inspectionMethodType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_INSPECTION_METHOD", Description = "Inspection Method", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var groupId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = groupType, Code = "MEC", Description = "Mechanical", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = dataType, Code = "DEC", Description = "Decimal", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = validationType, Code = "RNG", Description = "Range", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var inspMethodId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = inspectionMethodType, Code = "LAB", Description = "Lab Test", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var qpId = await new QualityParameterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.QualityParameter
                {
                    ParameterCode = "QP-000001",
                    ParameterName = "Tensile",
                    ParameterGroupId = groupId,
                    DataTypeId = dataTypeId,
                    ValidationTypeId = validationTypeId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });

            return (qpId, inspMethodId);
        }

        private Domain.Entities.QualityTemplate BuildTemplate(int qpId, int inspMethodId, string name = "Yarn QC") =>
            new Domain.Entities.QualityTemplate
            {
                TemplateCode = "QT-000001",
                TemplateName = name,
                Description = "Standard QC checks",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                QualityTemplateParameters = new List<Domain.Entities.QualityTemplateParameter>
                {
                    new Domain.Entities.QualityTemplateParameter
                    {
                        QualityParameterId = qpId,
                        SequenceNo = 1,
                        IsMandatory = true,
                        IsCritical = true,
                        InspectionMethodId = inspMethodId,
                        SampleSize = 5,
                        SampleUomId = 1,
                        IsGradeApplicable = true,
                        Remarks = "Lab test",
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(BuildTemplate(qpId, inspId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Detail()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(BuildTemplate(qpId, inspId, "Yarn QC"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualityTemplate
                .Include(t => t.QualityTemplateParameters)
                .FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.TemplateName.Should().Be("Yarn QC");
            saved.QualityTemplateParameters.Should().HaveCount(1);
            saved.QualityTemplateParameters!.First().QualityParameterId.Should().Be(qpId);
            saved.QualityTemplateParameters!.First().InspectionMethodId.Should().Be(inspId);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(BuildTemplate(qpId, inspId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualityTemplate.FirstOrDefaultAsync(x => x.Id == newId);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_DetailRows()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();

            int id;
            await using (var ctx1 = _fixture.CreateFreshDbContext())
            {
                id = await CreateRepo(ctx1).CreateAsync(BuildTemplate(qpId, inspId));
            }

            // Build new entity to send with REPLACE strategy
            var updateEntity = new Domain.Entities.QualityTemplate
            {
                Id = id,
                TemplateName = "Updated Name",
                Description = "Updated desc",
                IsActive = Status.Active,
                QualityTemplateParameters = new List<Domain.Entities.QualityTemplateParameter>
                {
                    new Domain.Entities.QualityTemplateParameter
                    {
                        QualityParameterId = qpId,
                        SequenceNo = 1,
                        IsMandatory = false,
                        IsCritical = false,
                        IsGradeApplicable = false,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };

            await using (var ctx2 = _fixture.CreateFreshDbContext())
            {
                await CreateRepo(ctx2).UpdateAsync(updateEntity);
            }

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var saved = await ctx3.QualityTemplate
                .Include(t => t.QualityTemplateParameters)
                .FirstAsync(x => x.Id == id);

            saved.TemplateName.Should().Be("Updated Name");
            saved.QualityTemplateParameters.Should().HaveCount(1);
            saved.QualityTemplateParameters!.First().IsMandatory.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_TemplateCode()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();

            int id;
            string originalCode;
            await using (var ctx1 = _fixture.CreateFreshDbContext())
            {
                id = await CreateRepo(ctx1).CreateAsync(BuildTemplate(qpId, inspId));
                originalCode = (await ctx1.QualityTemplate.FirstAsync(x => x.Id == id)).TemplateCode;
            }

            var updateEntity = new Domain.Entities.QualityTemplate
            {
                Id = id,
                TemplateName = "Renamed",
                IsActive = Status.Active,
                QualityTemplateParameters = new List<Domain.Entities.QualityTemplateParameter>()
            };

            await using (var ctx2 = _fixture.CreateFreshDbContext())
            {
                await CreateRepo(ctx2).UpdateAsync(updateEntity);
            }

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.QualityTemplate.FirstAsync(x => x.Id == id);
            updated.TemplateCode.Should().Be(originalCode);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();

            await using var ctx1 = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx1).CreateAsync(BuildTemplate(qpId, inspId));

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var ok = await CreateRepo(ctx2).SoftDeleteAsync(id, CancellationToken.None);

            ok.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SoftDelete_Header_And_Children()
        {
            await ClearAllAsync();
            var (qpId, inspId) = await SeedDependenciesAsync();

            await using var ctx1 = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx1).CreateAsync(BuildTemplate(qpId, inspId));

            await using (var ctx2 = _fixture.CreateFreshDbContext())
            {
                await CreateRepo(ctx2).SoftDeleteAsync(id, CancellationToken.None);
            }

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var header = await ctx3.QualityTemplate
                .IgnoreQueryFilters()
                .Include(t => t.QualityTemplateParameters)
                .FirstOrDefaultAsync(x => x.Id == id);

            header.Should().NotBeNull();
            header!.IsDeleted.Should().Be(IsDelete.Deleted);
            header.QualityTemplateParameters!.All(p => p.IsDeleted == IsDelete.Deleted).Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearAllAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var ok = await CreateRepo(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            ok.Should().BeFalse();
        }
    }
}
