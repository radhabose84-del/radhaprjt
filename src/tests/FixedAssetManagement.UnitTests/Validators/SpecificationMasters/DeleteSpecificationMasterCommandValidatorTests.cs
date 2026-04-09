using FAM.Application.Common.Interfaces.ISpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster;
using FAM.Presentation.Validation.SpecificationMaster;
using FluentValidation.TestHelper;

namespace FixedAssetManagement.UnitTests.Validators.SpecificationMasters
{
    public sealed class DeleteSpecificationMasterCommandValidatorTests
    {
        private readonly Mock<ISpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteSpecificationMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteSpecificationMasterCommand { Id = 1 };
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteSpecificationMasterCommand { Id = 0 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_LinkedRecords_FailsValidation()
        {
            var command = new DeleteSpecificationMasterCommand { Id = 1 };
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
