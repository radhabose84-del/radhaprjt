using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.DeleteRepackingHeader;
using ProductionManagement.Presentation.Validation.RepackingHeader;

namespace ProductionManagement.UnitTests.Validators.RepackingHeader
{
    public sealed class DeleteRepackingHeaderCommandValidatorTests
    {
        private readonly Mock<IRepackingHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteRepackingHeaderCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1, bool notFound = false, bool linked = false)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(notFound);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(linked);
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            SetupAllAsyncMocks(0);
            var result = await CreateValidator().TestValidateAsync(new DeleteRepackingHeaderCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            SetupAllAsyncMocks(999, notFound: true, linked: false);
            var result = await CreateValidator().TestValidateAsync(new DeleteRepackingHeaderCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_LinkedId_FailsWithSoftDeleteMessage()
        {
            SetupAllAsyncMocks(1, notFound: false, linked: true);
            var result = await CreateValidator().TestValidateAsync(new DeleteRepackingHeaderCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("This master is linked with other records. You cannot delete this record.");
        }

        [Fact]
        public async Task Validate_ExistingAndNotLinkedId_PassesValidation()
        {
            SetupAllAsyncMocks(1, notFound: false, linked: false);
            var result = await CreateValidator().TestValidateAsync(new DeleteRepackingHeaderCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
