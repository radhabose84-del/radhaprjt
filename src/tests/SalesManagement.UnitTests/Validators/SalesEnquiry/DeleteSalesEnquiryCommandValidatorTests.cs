using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.DeleteSalesEnquiry;
using SalesManagement.Presentation.Validation.SalesEnquiry;

namespace SalesManagement.UnitTests.Validators.SalesEnquiry
{
    public sealed class DeleteSalesEnquiryCommandValidatorTests
    {
        private readonly Mock<ISalesEnquiryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteSalesEnquiryCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesEnquiryCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesEnquiryCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesEnquiryCommand(1));
            result.ShouldHaveAnyValidationError();
        }
    }
}
