using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.UpdatePriceGroupMaster;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.PriceGroupMaster;

namespace InventoryManagement.UnitTests.Validators.PriceGroupMaster
{
    public sealed class UpdatePriceGroupMasterCommandValidatorTests
    {
        private readonly Mock<IPriceGroupMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);

        public UpdatePriceGroupMasterCommandValidatorTests()
        {
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
        }

        private UpdatePriceGroupMasterCommandValidator CreateValidator() =>
            new(_maxLengthProvider, _mockQueryRepo.Object);

        private static UpdatePriceGroupMasterCommand ValidCommand(
            int id = 1,
            string name = "Updated",
            int isActive = 1,
            DateTimeOffset? from = null,
            DateTimeOffset? to = null) =>
            new()
            {
                Id = id,
                PriceGroupName = name,
                Description = "desc",
                EffectiveFrom = from ?? new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EffectiveTo = to,
                IsActive = isActive
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand(id: 0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand(id: 99));
            result.ShouldHaveValidationErrorFor(x => x.Id);
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
        public async Task Validate_DuplicateName_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NameAlreadyExistsAsync("DupName", It.IsAny<int?>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand(name: "DupName"));
            result.ShouldHaveValidationErrorFor(x => x.PriceGroupName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_IsActive_OutOfRange_FailsValidation(int isActive)
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand(isActive: isActive));
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_EffectiveTo_BeforeFrom_FailsValidation()
        {
            var from = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var result = await CreateValidator().TestValidateAsync(ValidCommand(from: from, to: to));
            result.ShouldHaveValidationErrorFor(x => x.EffectiveTo);
        }
    }
}
