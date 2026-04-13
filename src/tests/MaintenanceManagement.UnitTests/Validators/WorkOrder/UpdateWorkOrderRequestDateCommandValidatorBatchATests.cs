using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.Maintenance.WorkOrder.Command.UpdateWorkOrderRequestDate;

namespace MaintenanceManagement.UnitTests.Validators.WorkOrder
{
    public sealed class UpdateWorkOrderRequestDateCommandValidatorBatchATests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateWorkOrderRequestDateCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public void Constructor_WithValidRepo_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_NegativeWorkOrderId_HasErrorForWorkOrderId()
        {
            var command = new UpdateWorkOrderRequestDateCommand
            {
                WorkOrderId = -1,
                RequestDate = DateTimeOffset.UtcNow,
                IsSystemTime = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WorkOrderId);
        }

        [Fact]
        public async Task Validate_InvalidRequestDateCheck_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.ValidateRequestDateAsync(
                    It.IsAny<int>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var command = new UpdateWorkOrderRequestDateCommand
            {
                WorkOrderId = 5,
                RequestDate = DateTimeOffset.UtcNow.AddDays(-10),
                IsSystemTime = 0
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
