using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Presentation.Validation;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.PortMaster
{
    public sealed class DeletePortMasterValidatorTests
    {
        private readonly Mock<IPortMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeletePortMasterValidator CreateValidator() => new(_mockQueryRepo.Object);

        private void SetupValid(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupValid(1);
            var command = PortMasterBuilders.ValidDeleteCommand(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidDeleteCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NegativeId_FailsValidation()
        {
            var command = PortMasterBuilders.ValidDeleteCommand(-1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
