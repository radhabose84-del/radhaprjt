using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Microsoft.Data.SqlClient;
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
    public sealed class QualitySpecificationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        // QcTypeId FK target — seeded by SeedDependenciesAsync, consumed by SeedSpecAsync.
        private int _qcTypeId;

        public QualitySpecificationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private QualitySpecificationQueryRepository CreateQueryRepo(
            Mock<IInventoryCategoryLookup> categoryLookup = null,
            Mock<IItemLookup> itemLookup = null)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new QualitySpecificationQueryRepository(
                conn,
                (categoryLookup ?? BuildDefaultCategoryLookup()).Object,
                (itemLookup ?? BuildDefaultItemLookup()).Object);
        }

        private Mock<IInventoryCategoryLookup> BuildDefaultCategoryLookup()
        {
            var mock = new Mock<IInventoryCategoryLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetCategoryByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new CategoryMasterDto { Id = id, ItemCategoryName = $"Cat{id}" }).ToList());
            return mock;
        }

        private Mock<IItemLookup> BuildDefaultItemLookup()
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                    ids.Select(id => new ItemLookupDto { Id = id, ItemCode = $"I{id}", ItemName = $"Item{id}" }).ToList());
            return mock;
        }

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
            var qcType = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster { MiscTypeCode = "QP_QC_TYPE", Description = "QC Type", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

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
            _qcTypeId = await miscRepo.CreateAsync(
                new Domain.Entities.MiscMaster { MiscTypeId = qcType, Code = "INPROCESS", Description = "In Process", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted });

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

        private async Task<int> SeedSpecAsync(
            int templateId, int qpId, int applicableLevelId, int validationTypeId,
            int severityId, int failureActionId, string code = "QS-0001", string name = "Cotton Spec",
            bool isActive = true, int? itemCategoryId = 5, int? itemId = null,
            DateTimeOffset? effectiveFrom = null, DateTimeOffset? effectiveTo = null,
            string allowedValues = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new QualitySpecificationCommandRepository(ctx).CreateAsync(
                new Domain.Entities.QualitySpecification
                {
                    SpecificationCode = code,
                    SpecificationName = name,
                    QualityTemplateId = templateId,
                    QcTypeId = _qcTypeId,
                    ApplicableLevelId = applicableLevelId,
                    ItemCategoryId = itemCategoryId,
                    ItemId = itemId,
                    EffectiveFrom = effectiveFrom ?? new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                    EffectiveTo = effectiveTo,
                    IsActive = isActive ? Status.Active : Status.Inactive,
                    IsDeleted = IsDelete.NotDeleted,
                    QualitySpecificationParameters = new List<Domain.Entities.QualitySpecificationParameter>
                    {
                        new Domain.Entities.QualitySpecificationParameter
                        {
                            QualityParameterId = qpId,
                            ValidationTypeId = validationTypeId,
                            MinValue = 39.5m,
                            MaxValue = 40.5m,
                            AllowedValues = allowedValues,
                            SeverityId = severityId,
                            FailureActionId = failureActionId,
                            IsSamplingRequired = true,
                            IsActive = Status.Active,
                            IsDeleted = IsDelete.NotDeleted
                        }
                    }
                });
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Records()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId);

            var (rows, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null, null, null, null, null, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_FullDetail()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            var id = await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.SpecificationCode.Should().Be("QS-0001");
            dto.QualityTemplateCode.Should().Be("QT-000001");
            dto.ApplicableLevelCode.Should().Be("ITEM CATEGORY");
            dto.Parameters.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_AllowedValues_SplitsByPipe()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            var id = await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId, allowedValues: "A|B|C");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto!.Parameters!.First().AllowedValues.Should().BeEquivalentTo(new[] { "A", "B", "C" });
        }

        [Fact]
        public async Task GetByIdAsync_NullAllowedValues_ReturnsEmptyList()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            var id = await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId, allowedValues: null);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto!.Parameters!.First().AllowedValues.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_NonExistent_ReturnsNull()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(9999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId, name: "Unique Spec");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("Unique Spec");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_NonExistent_ReturnsTrue()
        {
            await ClearAllAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task ApplicableLevelExistsAsync_Should_Return_True_For_Valid()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            var exists = await CreateQueryRepo().ApplicableLevelExistsAsync(deps.applicableLevelId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task GetApplicableLevelCodeAsync_Should_Return_Code()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            var code = await CreateQueryRepo().GetApplicableLevelCodeAsync(deps.applicableLevelId);

            code.Should().Be("ITEM CATEGORY");
        }

        [Fact]
        public async Task GetValidationTypeCodesByIdsAsync_Should_Return_Map()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();

            var map = await CreateQueryRepo().GetValidationTypeCodesByIdsAsync(new[] { deps.validationTypeId });

            map.Should().ContainKey(deps.validationTypeId);
            map[deps.validationTypeId].Should().Be("RNG");
        }

        [Fact]
        public async Task GetExistingParameterRowIdsAsync_Should_Return_All_Active_Rows()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            var id = await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId);

            var ids = await CreateQueryRepo().GetExistingParameterRowIdsAsync(id);

            ids.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSpecificationItemContextAsync_Should_Return_Item_Context()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            var id = await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId, itemCategoryId: 7, itemId: null);

            var (catId, itemId) = await CreateQueryRepo().GetSpecificationItemContextAsync(id);

            catId.Should().Be(7);
            itemId.Should().BeNull();
        }

        [Fact]
        public async Task HasOverlappingActiveSpecAsync_OverlappingDates_ReturnsTrue()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId,
                code: "QS-0001", itemCategoryId: 5,
                effectiveFrom: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                effectiveTo: new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero));

            var overlap = await CreateQueryRepo().HasOverlappingActiveSpecAsync(
                5, null,
                new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2027, 6, 1, 0, 0, 0, TimeSpan.Zero));

            overlap.Should().BeTrue();
        }

        [Fact]
        public async Task HasOverlappingActiveSpecAsync_NoOverlap_ReturnsFalse()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId,
                code: "QS-0001", itemCategoryId: 5,
                effectiveFrom: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                effectiveTo: new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero));

            var overlap = await CreateQueryRepo().HasOverlappingActiveSpecAsync(
                5, null,
                new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero));

            overlap.Should().BeFalse();
        }

        [Fact]
        public async Task HasOverlappingActiveSpecAsync_OpenEnded_HandledCorrectly()
        {
            // null EffectiveTo means open-ended (∞) — COALESCE handles it
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId,
                code: "QS-0001", itemCategoryId: 5,
                effectiveFrom: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                effectiveTo: null);  // open-ended

            var overlap = await CreateQueryRepo().HasOverlappingActiveSpecAsync(
                5, null,
                new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero),
                null);  // also open-ended

            overlap.Should().BeTrue();
        }

        [Fact]
        public async Task GetMaxSpecificationCodeSequenceAsync_Should_Return_Max()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId, code: "QS-0005", name: "Spec A");
            await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId, code: "QS-0003", name: "Spec B");

            var max = await CreateQueryRepo().GetMaxSpecificationCodeSequenceAsync();

            max.Should().Be(5);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Only()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId,
                code: "QS-0001", name: "Active Spec", isActive: true);
            await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId,
                code: "QS-0002", name: "Inactive Spec", isActive: false);

            var result = await CreateQueryRepo().AutocompleteAsync(string.Empty, CancellationToken.None);

            result.Should().HaveCount(1);
            result.First().SpecificationName.Should().Be("Active Spec");
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_NoDependents_ReturnsFalse()
        {
            await ClearAllAsync();
            var deps = await SeedDependenciesAsync();
            var id = await SeedSpecAsync(deps.templateId, deps.qpId, deps.applicableLevelId, deps.validationTypeId, deps.severityId, deps.failureActionId);

            var blocked = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            blocked.Should().BeFalse();
        }
    }
}
