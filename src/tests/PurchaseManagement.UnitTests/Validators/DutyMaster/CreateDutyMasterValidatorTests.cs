using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;
using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Command.Create;
using PurchaseManagement.Application.Purchase.DutyMaster.Validation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.DutyMaster
{
    public sealed class CreateDutyMasterValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IDutyMasterQueryRepository> _mockDutyRepo = new(MockBehavior.Loose);

        private CreateDutyMasterValidator CreateValidator() =>
            new(_mockMiscRepo.Object, _mockDutyRepo.Object);

        private void SetupAllAsyncMocks(int dutyCategoryId = 1)
        {
            _mockMiscRepo
                .Setup(r => r.NotFoundAsync(dutyCategoryId))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = DutyMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTariffNumber_FailsValidation(string? tariffNumber)
        {
            var command = DutyMasterBuilders.ValidCreateCommand(tariffNumber: tariffNumber!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Model.TariffNumber);
        }

        [Fact]
        public async Task Validate_TariffNumberTooLong_FailsValidation()
        {
            var command = DutyMasterBuilders.ValidCreateCommand(tariffNumber: new string('1', 51));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Model.TariffNumber);
        }

        [Fact]
        public async Task Validate_ZeroDutyCategoryId_FailsValidation()
        {
            var command = DutyMasterBuilders.ValidCreateCommand(dutyCategoryId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
