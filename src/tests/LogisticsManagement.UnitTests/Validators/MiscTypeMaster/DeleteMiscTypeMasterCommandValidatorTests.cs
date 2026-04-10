using FluentValidation.TestHelper;
using LogisticsManagement.Application.Common.Interfaces.IMiscTypeMaster;
using LogisticsManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using LogisticsManagement.Presentation.Validation.MiscTypeMaster;

namespace LogisticsManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class DeleteMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(It.IsAny<int>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            SetupAllValid();

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            SetupAllValid();

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(99));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_LinkedRecordsExist_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(1));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
