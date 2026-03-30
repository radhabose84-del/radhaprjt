using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup;
using SalesManagement.Presentation.Validation.SalesGroup;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesGroup
{
    public class CreateSalesGroupCommandValidatorTests
    {
        private readonly Mock<ISalesGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateSalesGroupCommandValidator CreateValidator()
            => new CreateSalesGroupCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Sets up ALL async mocks for a happy-path command.
        /// FluentValidation runs ALL rules — every async mock must be satisfied.
        /// </summary>
        private void SetupAllAsyncMocks(
            string name = "Test Sales Group",
            int salesOfficeId = 1,
            int? productCategoryId = null)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(name, salesOfficeId, null))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.SalesOfficeExistsAsync(salesOfficeId))
                .ReturnsAsync(true);

            if (productCategoryId.HasValue && productCategoryId.Value > 0)
            {
                _mockQueryRepo
                    .Setup(r => r.ProductCategoryExistsAsync(productCategoryId.Value, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);
            }
        }

        private void SetupSalesOfficeExists(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.SalesOfficeExistsAsync(id))
                .ReturnsAsync(true);
        }

        private void SetupSalesOfficeNotFound(int id)
        {
            _mockQueryRepo
                .Setup(r => r.SalesOfficeExistsAsync(id))
                .ReturnsAsync(false);
        }

        private void SetupAlreadyExistsAny(bool exists = false)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null))
                .ReturnsAsync(exists);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = SalesGroupBuilders.ValidCreateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── SalesGroupName Rules ──────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesGroupName_Empty_FailsValidation(string? name)
        {
            var command = SalesGroupBuilders.ValidCreateCommand(name: name!);
            // SalesOfficeId > 0 so FK check fires, but name is empty so AlreadyExists .When guard skips

            _mockQueryRepo
                .Setup(r => r.SalesOfficeExistsAsync(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroupName);
        }

        [Fact]
        public async Task SalesGroupName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = SalesGroupBuilders.ValidCreateCommand(name: longName);
            SetupSalesOfficeExists();
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroupName);
        }

        [Fact]
        public async Task SalesGroupName_MaxLength100_PassesValidation()
        {
            var maxName = new string('A', 100);
            var command = SalesGroupBuilders.ValidCreateCommand(name: maxName);
            SetupAllAsyncMocks(name: maxName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.SalesGroupName);
        }

        [Fact]
        public async Task SalesGroupName_AlreadyExists_FailsValidation()
        {
            var command = SalesGroupBuilders.ValidCreateCommand(name: "Duplicate Group");
            SetupSalesOfficeExists();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("Duplicate Group", 1, null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            // AlreadyExists is on the model-level rule (RuleFor(x => x)), not on SalesGroupName property directly
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists."));
        }

        // ── SalesOfficeId Rules ───────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SalesOfficeId_ZeroOrNegative_FailsValidation(int salesOfficeId)
        {
            var command = SalesGroupBuilders.ValidCreateCommand(salesOfficeId: salesOfficeId);
            // .When(x => x.SalesOfficeId > 0) guards FK check, so no FK mock needed
            // AlreadyExists .When guard also skips when SalesOfficeId <= 0

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeId);
        }

        [Fact]
        public async Task SalesOfficeId_NotFound_FailsValidation()
        {
            var command = SalesGroupBuilders.ValidCreateCommand(salesOfficeId: 999);
            SetupSalesOfficeNotFound(999);
            SetupAlreadyExistsAny();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOfficeId);
        }

        [Fact]
        public async Task SalesOfficeId_Exists_PassesValidation()
        {
            var command = SalesGroupBuilders.ValidCreateCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.SalesOfficeId);
        }

        // ── ProductCategoryId Rules ───────────────────────────────────────────

        [Fact]
        public async Task ProductCategoryId_Null_PassesValidation()
        {
            var command = SalesGroupBuilders.ValidCreateCommand(productCategoryId: null);
            SetupAllAsyncMocks(productCategoryId: null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.ProductCategoryId);
        }

        [Fact]
        public async Task ProductCategoryId_ValidValue_PassesValidation()
        {
            var command = SalesGroupBuilders.ValidCreateCommand(productCategoryId: 5);
            SetupAllAsyncMocks(productCategoryId: 5);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.ProductCategoryId);
        }

        [Fact]
        public async Task ProductCategoryId_NotFound_FailsValidation()
        {
            var command = SalesGroupBuilders.ValidCreateCommand(productCategoryId: 999);
            SetupSalesOfficeExists();
            SetupAlreadyExistsAny();
            _mockQueryRepo
                .Setup(r => r.ProductCategoryExistsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ProductCategoryId);
        }

        // ── Optional MaxLength Fields ─────────────────────────────────────────

        [Fact]
        public async Task ResponsibleManager_TooLong_FailsValidation()
        {
            var longManager = new string('A', 101);
            var command = SalesGroupBuilders.ValidCreateCommand(responsibleManager: longManager);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ResponsibleManager);
        }

        [Fact]
        public async Task ResponsibleManager_Empty_PassesValidation()
        {
            var command = SalesGroupBuilders.ValidCreateCommand(responsibleManager: null!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.ResponsibleManager);
        }

        [Fact]
        public async Task RegionTerritory_TooLong_FailsValidation()
        {
            var longRegion = new string('A', 101);
            var command = SalesGroupBuilders.ValidCreateCommand(regionTerritory: longRegion);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RegionTerritory);
        }

        [Fact]
        public async Task RegionTerritory_Empty_PassesValidation()
        {
            var command = SalesGroupBuilders.ValidCreateCommand(regionTerritory: null!);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.RegionTerritory);
        }
    }
}
