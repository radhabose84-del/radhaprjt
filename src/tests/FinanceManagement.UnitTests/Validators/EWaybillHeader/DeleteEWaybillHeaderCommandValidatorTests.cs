using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.DeleteEWaybillHeader;
using FinanceManagement.Presentation.Validation.EWaybillHeader;

namespace FinanceManagement.UnitTests.Validators.EWaybillHeader
{
    public sealed class DeleteEWaybillHeaderCommandValidatorTests
    {
        private readonly Mock<IEWaybillHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteEWaybillHeaderCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteEWaybillHeaderCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = new DeleteEWaybillHeaderCommand(99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var command = new DeleteEWaybillHeaderCommand(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
