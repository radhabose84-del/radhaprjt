using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;
using SalesManagement.Presentation.Validation.SalesGroup;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesGroup
{
    public class UpdateSalesGroupCommandValidatorTests
    {
        private readonly Mock<ISalesGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateSalesGroupCommandValidator CreateValidator()
            => new UpdateSalesGroupCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupAllAsyncMocks(
            int id = 1,
            string name = "Updated Sales Group",
            int salesOfficeId = 1,
            int? productCategoryId = null)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.SalesOfficeExistsAsync(salesOfficeId))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(name, salesOfficeId, id))
                .ReturnsAsync(false);

            if (productCategoryId.HasValue && productCategoryId.Value > 0)
            {
                _mockQueryRepo
                    .Setup(r => r.ProductCategoryExistsAsync(productCategoryId.Value, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
            }
        }

        private void SetupIdExists(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);
        }

        private void SetupIdNotFound(int id = 99)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        private void SetupSalesOfficeExists(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.SalesOfficeExistsAsync(id))
                .ReturnsAsync(true);
        }

        private void SetupAlreadyExistsAny(bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(exists);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = SalesGroupBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ──────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);

            var command = SalesGroupBuilders.ValidUpdateCommand(id: id);
            // SalesOfficeId > 0, so FK check fires
            SetupSalesOfficeExists();
            // AlreadyExists .When guard requires Id > 0, so it skips

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var command = SalesGroupBuilders.ValidUpdateCommand(id: 99);
            SetupIdNotFound(99);
            SetupSalesOfficeExists();
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── SalesGroupName Rules ──────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesGroupName_Empty_FailsValidation(string? name)
        {
            var command = SalesGroupBuilders.ValidUpdateCommand(name: name!);
            SetupIdExists();
            SetupSalesOfficeExists();
            // AlreadyExists .When guard skips when name is empty

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroupName);
        }

        [Fact]
        public async Task SalesGroupName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = SalesGroupBuilders.ValidUpdateCommand(name: longName);
            SetupIdExists();
            SetupSalesOfficeExists();
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroupName);
        }

        // ── SalesOfficeId Rules ───────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SalesOfficeId_ZeroOrNegative_FailsValidation(int salesOfficeId)
        {
            var command = SalesGroupBuilders.ValidUpdateCommand(salesOfficeId: salesOfficeId);
            SetupIdExists();
            // .When(x => x.SalesOfficeId > 0) guards FK check — skips
            // AlreadyExists .When guard requires SalesOfficeId > 0 — skips

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeId);
        }

        [Fact]
        public async Task SalesOfficeId_NotFound_FailsValidation()
        {
            var command = SalesGroupBuilders.ValidUpdateCommand(salesOfficeId: 999);
            SetupIdExists();
            _mockQueryRepo
                .Setup(r => r.SalesOfficeExistsAsync(999))
                .ReturnsAsync(false);
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeId);
        }

        // ── ProductCategoryId Rules ───────────────────────────────────────────

        [Fact]
        public async Task ProductCategoryId_Null_PassesValidation()
        {
            var command = SalesGroupBuilders.ValidUpdateCommand(productCategoryId: null);
            SetupAllAsyncMocks(productCategoryId: null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.ProductCategoryId);
        }

        [Fact]
        public async Task ProductCategoryId_NotFound_FailsValidation()
        {
            var command = SalesGroupBuilders.ValidUpdateCommand(productCategoryId: 999);
            SetupIdExists();
            SetupSalesOfficeExists();
            SetupAlreadyExistsAny();
            _mockQueryRepo
                .Setup(r => r.ProductCategoryExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ProductCategoryId);
        }

        // ── AlreadyExists (Composite Key) ────────────────────────────────────

        [Fact]
        public async Task SalesGroupName_AlreadyExists_FailsValidation()
        {
            var command = SalesGroupBuilders.ValidUpdateCommand(name: "Duplicate Group");
            SetupIdExists();
            SetupSalesOfficeExists();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("Duplicate Group", 1, 1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists."));
        }

        // ── Optional MaxLength Fields ─────────────────────────────────────────

        [Fact]
        public async Task ResponsibleManager_TooLong_FailsValidation()
        {
            var longManager = new string('A', 101);
            var command = SalesGroupBuilders.ValidUpdateCommand(responsibleManager: longManager);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ResponsibleManager);
        }

        [Fact]
        public async Task RegionTerritory_TooLong_FailsValidation()
        {
            var longRegion = new string('A', 101);
            var command = SalesGroupBuilders.ValidUpdateCommand(regionTerritory: longRegion);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RegionTerritory);
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            var command = SalesGroupBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var command = SalesGroupBuilders.ValidUpdateCommand(isActive: isActive);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
