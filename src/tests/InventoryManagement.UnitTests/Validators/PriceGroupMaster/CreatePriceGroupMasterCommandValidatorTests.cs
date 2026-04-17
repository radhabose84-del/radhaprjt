using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.CreatePriceGroupMaster;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.PriceGroupMaster;

namespace InventoryManagement.UnitTests.Validators.PriceGroupMaster
{
    public sealed class CreatePriceGroupMasterCommandValidatorTests
    {
        private readonly Mock<IPriceGroupMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        // null DbContext keeps MaxLengthProvider in the "no model" branch → GetMaxLength returns null → validator falls back to defaults.
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);

        public CreatePriceGroupMasterCommandValidatorTests()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
        }

        private CreatePriceGroupMasterCommandValidator CreateValidator() =>
            new(_maxLengthProvider, _mockQueryRepo.Object);

        private static CreatePriceGroupMasterCommand ValidCommand(
            string code = "PG001",
            string name = "Standard",
            DateTimeOffset? from = null,
            DateTimeOffset? to = null) =>
            new()
            {
                PriceGroupCode = code,
                PriceGroupName = name,
                Description = "desc",
                EffectiveFrom = from ?? new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EffectiveTo = to
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand(code: code!));
            result.ShouldHaveValidationErrorFor(x => x.PriceGroupCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand(name: name!));
            result.ShouldHaveValidationErrorFor(x => x.PriceGroupName);
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("PG001", It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand(code: "PG001"));
            result.ShouldHaveValidationErrorFor(x => x.PriceGroupCode);
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync("Standard", It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand(name: "Standard"));
            result.ShouldHaveValidationErrorFor(x => x.PriceGroupName);
        }

        [Theory]
        [InlineData("PG-01")]   // hyphen
        [InlineData("PG 01")]   // space
        [InlineData("PG@01")]   // special char
        public async Task Validate_NonAlphanumericCode_FailsValidation(string code)
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand(code: code));
            result.ShouldHaveValidationErrorFor(x => x.PriceGroupCode);
        }

        [Fact]
        public async Task Validate_EffectiveTo_Before_EffectiveFrom_FailsValidation()
        {
            var from = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var result = await CreateValidator().TestValidateAsync(ValidCommand(from: from, to: to));
            result.ShouldHaveValidationErrorFor(x => x.EffectiveTo);
        }

        [Fact]
        public async Task Validate_EffectiveTo_Null_Passes()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand(to: null));
            result.ShouldNotHaveValidationErrorFor(x => x.EffectiveTo);
        }
    }
}
