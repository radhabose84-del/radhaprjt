using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Presentation.Validation.MaintenanceRequest;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceRequest
{
    public sealed class CreateMaintenanceRequestCommandValidatorTests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroRequestTypeId_FailsValidation()
        {
            var validator = new CreateMaintenanceRequestCommandValidator(_mockQueryRepo.Object);
            var command = new CreateMaintenanceRequestCommand { RequestTypeId = 0 };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class UpdateMaintenanceRequestCommandValidatorTests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var validator = new UpdateMaintenanceRequestCommandValidator(_mockQueryRepo.Object);
            var command = new UpdateMaintenanceRequestCommand { Id = 0 };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
