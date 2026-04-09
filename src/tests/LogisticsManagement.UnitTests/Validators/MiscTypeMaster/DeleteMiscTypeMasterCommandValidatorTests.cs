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

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(99));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_LinkedRecordsExist_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand(1));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
