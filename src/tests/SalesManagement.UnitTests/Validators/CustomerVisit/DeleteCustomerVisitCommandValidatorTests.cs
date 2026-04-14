using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisit;
using SalesManagement.Presentation.Validation.CustomerVisit;

namespace SalesManagement.UnitTests.Validators.CustomerVisit
{
    public sealed class DeleteCustomerVisitCommandValidatorTests
    {
        private readonly Mock<ICustomerVisitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

        private DeleteCustomerVisitCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object, _mockAccessFilter.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(new DeleteCustomerVisitCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotEmpty Rules ───────────────────────────────────────────────

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteCustomerVisitCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── NotFound Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteCustomerVisitCommand(1));

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task EntityExists_PassesNotFoundCheck()
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(new DeleteCustomerVisitCommand(1));

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
