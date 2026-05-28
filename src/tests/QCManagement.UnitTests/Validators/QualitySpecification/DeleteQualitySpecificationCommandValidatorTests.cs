using FluentValidation.TestHelper;
using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.DeleteQualitySpecification;
using QCManagement.Presentation.Validation.QualitySpecification;

namespace QCManagement.UnitTests.Validators.QualitySpecification
{
    public class DeleteQualitySpecificationCommandValidatorTests
    {
        private readonly Mock<IQualitySpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteQualitySpecificationCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteQualitySpecificationCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            // FluentValidation runs all rules — async ones still get called for Id=0
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteQualitySpecificationCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NonExistentId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteQualitySpecificationCommand(99));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task LinkedToDependents_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteQualitySpecificationCommand(1));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("This master is linked with other records. You cannot delete this record.");
        }
    }
}
