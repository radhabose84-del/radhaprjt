using AutoMapper;
using Contracts.Common;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UpdateMiscTypeMasterCommand ValidCommand() => new()
        {
            Id = 1, MiscTypeCode = "MT01", Description = "Updated MiscType", IsActive = 1
        };

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.IsMiscTypeMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((MaintenanceManagement.Domain.Entities.MiscTypeMaster?)null);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<UpdateMiscTypeMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MiscTypeMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsyncOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MiscTypeMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateCode_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.IsMiscTypeMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscTypeMaster());
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<UpdateMiscTypeMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MiscTypeMaster());

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_UpdateReturnsFalse_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.IsMiscTypeMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((MaintenanceManagement.Domain.Entities.MiscTypeMaster?)null);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<UpdateMiscTypeMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MiscTypeMaster());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<MaintenanceManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_InactivateLinkedRecord_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.IsMiscTypeMasterLinkedAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<UpdateMiscTypeMasterCommand>()))
                .Returns(new MaintenanceManagement.Domain.Entities.MiscTypeMaster());

            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MT01", IsActive = 0 };
            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
