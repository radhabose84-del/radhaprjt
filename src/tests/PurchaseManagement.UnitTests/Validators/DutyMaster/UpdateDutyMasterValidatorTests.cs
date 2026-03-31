using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Purchase.DutyMaster.Command.Update;
using PurchaseManagement.Application.Purchase.DutyMaster.Validation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.DutyMaster
{
    public sealed class UpdateDutyMasterValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Strict);

        private UpdateDutyMasterValidator CreateValidator() =>
            new(_mockMiscRepo.Object);

        private void SetupExistsTrue(int id = 1)
        {
            _mockMiscRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = DutyMasterBuilders.ValidUpdateCommand(1);
            SetupExistsTrue(command.Model.DutyCategoryId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = DutyMasterBuilders.ValidUpdateCommand(0);
            SetupExistsTrue(command.Model.DutyCategoryId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Model.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTariffNumber_FailsValidation(string? tariffNumber)
        {
            var command = DutyMasterBuilders.ValidUpdateCommand(1);
            command.Model.TariffNumber = tariffNumber!;
            SetupExistsTrue(command.Model.DutyCategoryId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Model.TariffNumber);
        }

        [Fact]
        public async Task Validate_InvalidDutyCategoryId_FailsValidation()
        {
            var command = DutyMasterBuilders.ValidUpdateCommand(1);
            _mockMiscRepo
                .Setup(r => r.NotFoundAsync(command.Model.DutyCategoryId))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Model.DutyCategoryId);
        }
    }
}
