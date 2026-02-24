#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new UpdateMiscTypeMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        private void SetupHappyPath(UpdateMiscTypeMasterCommand command, int updatedId = 1)
        {
            var existingDto = MiscTypeMasterBuilders.ValidDto(id: command.Id);
            var entity = MiscTypeMasterBuilders.ValidEntity(command.Id);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.MiscTypeMaster>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(updatedId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(id: 5);
            SetupHappyPath(command, updatedId: 5);
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Data.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "MISC_TYPE_UPDATE" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsEntityNotFoundException()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.MiscTypeMaster.Dto.MiscTypeMasterDto)null);
            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallUpdateAsync()
        {
            var command = MiscTypeMasterBuilders.ValidUpdateCommand(id: 99);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.MiscTypeMaster.Dto.MiscTypeMasterDto)null);
            var sut = CreateSut();

            try { await sut.Handle(command, CancellationToken.None); } catch { }

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Never);
        }
    }
}
