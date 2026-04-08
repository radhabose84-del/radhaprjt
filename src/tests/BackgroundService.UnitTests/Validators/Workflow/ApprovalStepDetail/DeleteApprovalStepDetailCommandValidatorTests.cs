using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.DeleteApprovalStepDetail;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using BackgroundService.Presentation.Validation.Workflow.ApprovalStepDetail;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Workflow.ApprovalStepDetail
{
    public sealed class DeleteApprovalStepDetailCommandValidatorTests
    {
        private readonly Mock<IApprovalStepDetailQuery> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteApprovalStepDetailCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(
                new DeleteApprovalStepDetailCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            // NotFound async rule still runs for Id=0
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(0))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(
                new DeleteApprovalStepDetailCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(99))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(
                new DeleteApprovalStepDetailCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
