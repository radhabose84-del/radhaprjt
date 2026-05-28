using Microsoft.EntityFrameworkCore;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.MiscMaster;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.Infrastructure.Repositories.QualityParameter;
using QCManagement.Infrastructure.Repositories.QualitySpecification;
using QCManagement.Infrastructure.Repositories.QualityTemplate;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.QualitySpecification
{
    [Collection("DatabaseCollection")]
    public sealed class QualitySpecificationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QualitySpecificationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private QualitySpecificationCommandRepository CreateRepo(ApplicationDbContext ctx)
            => new QualitySpecificationCommandRepository(ctx);

        private async Task ClearAllAsync() =>
            await _fixture.ClearTablesAsync(
                "QC.QualitySpecificationParameter", "QC.QualitySpecification",
                "QC.QualityTemplateParameter", "QC.QualityTemplate",
                "QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        private async Task<(int templateId, int qpId, int applicableLevelId, int validationTypeId, int severityId, int failureActionId)> SeedDependenciesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var groupType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_GROUP", Description = "Group", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_DATATYPE", Description = "Data Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_VALIDATION", Description = "Validation", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var applicableLevelType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_APPLICABLE_LEVEL", Description = "Applicable Level", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var severityType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_SEVERITY", Description = "Severity", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var actionType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_FAILURE_ACTION", Description = "Failure Action", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var groupId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = groupType, Code = "MEC", Description = "Mechanical", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var dataTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = dataType, Code = "DEC", Description = "Decimal", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var validationTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = validationType, Code = "RNG", Description = "Range", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var applicableLevelId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = applicableLevelType, Code = "ITEM CATEGORY", Description = "Item Category", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var severityId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = severityType, Code = "CRT", Description = "Critical", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });
            var failureActionId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = actionType, Code = "REJECT", Description = "Reject", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

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

            var templateId = await new QualityTemplateCommandRepository(ctx).CreateAsync(
                new Domain.Entities.QualityTemplate
                {
                    TemplateCode = "QT-000001",
                    TemplateName = "Yarn Template",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted,
                    QualityTemplateParameters = new List<Domain.Entities.QualityTemplateParameter>
                    {
                        new Domain.Entities.QualityTemplateParameter
                        {
                            QualityParameterId = qpId,
                            SequenceNo = 1,
                            IsMandatory = true,
                            IsActive = Status.Active,
                            IsDeleted = IsDelete.NotDeleted
                        }
                    }
                });

            return (templateId, qpId, applicableLevelId, validationTypeId, severityId, failureActionId);
        }

        private Domain.Entities.QualitySpecification BuildSpec(
            int templateId, int qpId, int applicableLevelId, int validationTypeId,
            int severityId, int failureActionId, string code = "QS-0001")
        {
            return new Domain.Entities.QualitySpecification
            {
                SpecificationCode = code,
                SpecificationName = "Cotton 40s Spec v1",
                QualityTemplateId = templateId,
                ApplicableLevelId = applicableLevelId,
                ItemCategoryId = 5,
                Description = "Test spec",
                EffectiveFrom = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                QualitySpecificationParameters = new List<Domain.Entities.QualitySpecificationParameter>
                {
                    new Domain.Entities.QualitySpecificationParameter
                    {
                        QualityParameterId = qpId,
                        ValidationTypeId = validationTypeId,
                        MinValue = 39.5m,
                        MaxValue = 40.5m,
                        AllowedValues = null,
                        SeverityId = severityId,
                        FailureActionId = failureActionId,
                        IsSamplingRequired = true,
                        Remarks = "Test",
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };
        }

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(
                BuildSpec(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Detail()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(
                BuildSpec(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualitySpecification
                .Include(t => t.QualitySpecificationParameters)
                .FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.SpecificationName.Should().Be("Cotton 40s Spec v1");
            saved.QualitySpecificationParameters.Should().HaveCount(1);
            saved.QualitySpecificationParameters!.First().MinValue.Should().Be(39.5m);
            saved.QualitySpecificationParameters!.First().MaxValue.Should().Be(40.5m);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(
                BuildSpec(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualitySpecification.FirstOrDefaultAsync(x => x.Id == newId);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_PerRow_Update()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            int id;
            int rowId;
            await using (var ctx1 = _fixture.CreateFreshDbContext())
            {
                id = await CreateRepo(ctx1).CreateAsync(
                    BuildSpec(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId));
                rowId = (await ctx1.QualitySpecificationParameter.FirstAsync(x => x.QualitySpecificationId == id)).Id;
            }

            // Update with per-row Id, changing MinValue
            var updateEntity = new Domain.Entities.QualitySpecification
            {
                Id = id,
                SpecificationName = "Updated Name",
                Description = "Updated",
                EffectiveFrom = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
                IsActive = Status.Active,
                QualitySpecificationParameters = new List<Domain.Entities.QualitySpecificationParameter>
                {
                    new Domain.Entities.QualitySpecificationParameter
                    {
                        Id = rowId,
                        ValidationTypeId = deps.validationTypeId,
                        MinValue = 38.0m,
                        MaxValue = 42.0m,
                        SeverityId = deps.severityId,
                        FailureActionId = deps.failureActionId,
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
            var saved = await ctx3.QualitySpecification
                .Include(t => t.QualitySpecificationParameters)
                .FirstAsync(x => x.Id == id);

            saved.SpecificationName.Should().Be("Updated Name");
            saved.QualitySpecificationParameters!.First().MinValue.Should().Be(38.0m);
            saved.QualitySpecificationParameters!.First().MaxValue.Should().Be(42.0m);
            saved.QualitySpecificationParameters!.First().Id.Should().Be(rowId);  // SAME row id (per-row UPDATE, not REPLACE)
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_SpecificationCode()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            int id;
            string originalCode;
            await using (var ctx1 = _fixture.CreateFreshDbContext())
            {
                id = await CreateRepo(ctx1).CreateAsync(
                    BuildSpec(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId));
                originalCode = (await ctx1.QualitySpecification.FirstAsync(x => x.Id == id)).SpecificationCode;
            }

            var updateEntity = new Domain.Entities.QualitySpecification
            {
                Id = id,
                SpecificationName = "Renamed",
                EffectiveFrom = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                IsActive = Status.Active,
                QualitySpecificationParameters = new List<Domain.Entities.QualitySpecificationParameter>()
            };

            await using (var ctx2 = _fixture.CreateFreshDbContext())
            {
                await CreateRepo(ctx2).UpdateAsync(updateEntity);
            }

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var updated = await ctx3.QualitySpecification.FirstAsync(x => x.Id == id);
            updated.SpecificationCode.Should().Be(originalCode);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            await using var ctx1 = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx1).CreateAsync(
                BuildSpec(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId));

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var ok = await CreateRepo(ctx2).SoftDeleteAsync(id, CancellationToken.None);

            ok.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SoftDelete_Header_And_Children()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            await using var ctx1 = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx1).CreateAsync(
                BuildSpec(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId));

            await using (var ctx2 = _fixture.CreateFreshDbContext())
            {
                await CreateRepo(ctx2).SoftDeleteAsync(id, CancellationToken.None);
            }

            await using var ctx3 = _fixture.CreateFreshDbContext();
            var header = await ctx3.QualitySpecification
                .IgnoreQueryFilters()
                .Include(t => t.QualitySpecificationParameters)
                .FirstOrDefaultAsync(x => x.Id == id);

            header.Should().NotBeNull();
            header!.IsDeleted.Should().Be(IsDelete.Deleted);
            header.QualitySpecificationParameters!.All(p => p.IsDeleted == IsDelete.Deleted).Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await ClearAllAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var ok = await CreateRepo(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            ok.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAsync_AllowedValues_PipeDelimited()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            var spec = BuildSpec(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId);
            spec.QualitySpecificationParameters!.First().AllowedValues = "A|B|C";

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(spec);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualitySpecificationParameter.FirstAsync(x => x.QualitySpecificationId == newId);
            saved.AllowedValues.Should().Be("A|B|C");
        }
    }
}
