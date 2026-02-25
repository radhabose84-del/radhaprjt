#nullable disable
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Presentation.Validation.SalesChannel;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesChannel
{
    public class CreateSalesChannelCommandValidatorTests
    {
        private readonly Mock<ISalesChannelQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateSalesChannelCommandValidator CreateValidator()
            => new CreateSalesChannelCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupCodeNotExists(string code = "CH001")
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
                .ReturnsAsync(false);
        }

        private void SetupCodeAlreadyExists(string code)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
                .ReturnsAsync(true);
        }

        /// <summary>
        /// Use when the test doesn't care about uniqueness.
        /// Needed because FluentValidation runs ALL rules regardless of earlier failures.
        /// </summary>
        private void SetupAnyCodeNotExists()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var command = SalesChannelBuilders.ValidCreateCommand();
            SetupCodeNotExists("CH001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── SalesChannelCode Rules ────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesChannelCode_Empty_FailsValidation(string code)
        {
            var command = SalesChannelBuilders.ValidCreateCommand(code: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelCode)
                  .WithErrorMessage("SalesChannelCode is required.");
        }

        [Fact]
        public async Task SalesChannelCode_TooLong_FailsValidation()
        {
            // 21 chars (max is 20); AlreadyExistsAsync still fires (When = !IsNullOrWhiteSpace)
            var longCode = new string('A', 21);
            var command = SalesChannelBuilders.ValidCreateCommand(code: longCode);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelCode)
                  .WithErrorMessage("SalesChannelCode  cannot be longer than   20 characters.");
        }

        [Theory]
        [InlineData("CH-001")]  // hyphen
        [InlineData("CH 001")]  // space
        [InlineData("CH.001")]  // dot
        [InlineData("CH_001")]  // underscore
        [InlineData("CH@001")]  // at sign
        public async Task SalesChannelCode_NotAlphanumeric_FailsValidation(string code)
        {
            // AlreadyExistsAsync still fires even when Matches rule fails
            var command = SalesChannelBuilders.ValidCreateCommand(code: code);
            SetupAnyCodeNotExists();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelCode)
                  .WithErrorMessage("SalesChannelCode  must be alphanumeric only.");
        }

        [Fact]
        public async Task SalesChannelCode_AlreadyExists_FailsValidation()
        {
            var command = SalesChannelBuilders.ValidCreateCommand(code: "CH001");
            SetupCodeAlreadyExists("CH001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelCode)
                  .WithErrorMessage("SalesChannelCode already exists.");
        }

        [Fact]
        public async Task SalesChannelCode_Unique_PassesExistenceCheck()
        {
            var command = SalesChannelBuilders.ValidCreateCommand(code: "CH999");
            SetupCodeNotExists("CH999");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.SalesChannelCode);
        }

        [Fact]
        public async Task SalesChannelCode_MaxLength20_PassesValidation()
        {
            var maxCode = new string('A', 20);
            var command = SalesChannelBuilders.ValidCreateCommand(code: maxCode);
            SetupCodeNotExists(maxCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.SalesChannelCode);
        }

        // ── SalesChannelName Rules ────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SalesChannelName_Empty_FailsValidation(string name)
        {
            var command = SalesChannelBuilders.ValidCreateCommand(name: name);
            SetupCodeNotExists("CH001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelName)
                  .WithErrorMessage("SalesChannelName is required.");
        }

        [Fact]
        public async Task SalesChannelName_TooLong_FailsValidation()
        {
            var longName = new string('A', 101);
            var command = SalesChannelBuilders.ValidCreateCommand(name: longName);
            SetupCodeNotExists("CH001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelName)
                  .WithErrorMessage("SalesChannelName  cannot be longer than   100 characters.");
        }

        [Fact]
        public async Task SalesChannelName_MaxLength100_PassesValidation()
        {
            var maxName = new string('A', 100);
            var command = SalesChannelBuilders.ValidCreateCommand(name: maxName);
            SetupCodeNotExists("CH001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.SalesChannelName);
        }
    }
}
