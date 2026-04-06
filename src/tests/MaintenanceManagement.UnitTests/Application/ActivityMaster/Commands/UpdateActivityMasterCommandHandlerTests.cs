using AutoMapper;
using Contracts.Common;
using FluentValidation;
using FluentValidation.Results;
using MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Application.ActivityMaster.Commands
{
    public sealed class UpdateActivityMasterCommandHandlerTests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IActivityMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IValidator<UpdateActivityMasterCommand>> _mockValidator = new(MockBehavior.Loose);

        private UpdateActivityMasterCommandHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockValidator.Object);

        private static UpdateActivityMasterCommand ValidCommand() => new()
        {
            UpdateActivityMaster = new UpdateActivityMasterDto
            {
                ActivityId = 1,
                ActivityName = "Updated Activity",
                IsActive = Status.Active
            }
        };

        private void SetupHappyPath(int updateResult = 1)
        {
            _mockQueryRepo.Setup(r => r.IsActivityMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ActivityMaster>(It.IsAny<UpdateActivityMasterDto>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ActivityMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<UpdateActivityMasterDto>()))
                .ReturnsAsync(updateResult);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsPositiveResult()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<UpdateActivityMasterDto>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateReturnsZero_ThrowsExceptionRules()
        {
            SetupHappyPath(0);
            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_InactivateLinkedRecord_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.IsActivityMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.ActivityMaster>(It.IsAny<UpdateActivityMasterDto>()))
                .Returns(new MaintenanceManagement.Domain.Entities.ActivityMaster());

            var command = new UpdateActivityMasterCommand
            {
                UpdateActivityMaster = new UpdateActivityMasterDto
                {
                    ActivityId = 1,
                    ActivityName = "Updated Activity",
                    IsActive = Status.Inactive
                }
            };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
