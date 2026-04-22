using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Application.PriceMaster.Commands.Delete;
using PurchaseManagement.Application.PriceMaster.Dtos;
using PurchaseManagement.Presentation.Validation.PriceMaster;

namespace PurchaseManagement.UnitTests.Validators.PriceMaster
{
    public sealed class DeletePriceMasterCommandValidatorTests
    {
        private readonly Mock<IPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeletePriceMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupEntityExists(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PriceMasterGetAllDto { Id = id });
        }

        private void SetupEntityNotFound(int id = 99)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PriceMasterGetAllDto?)null);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            SetupEntityExists(1);

            var command = new DeletePriceMasterCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            // FluentValidation runs all rules including async ones for Id=0
            SetupEntityNotFound(0);

            var command = new DeletePriceMasterCommand { Id = 0 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            SetupEntityNotFound(99);

            var command = new DeletePriceMasterCommand { Id = 99 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
