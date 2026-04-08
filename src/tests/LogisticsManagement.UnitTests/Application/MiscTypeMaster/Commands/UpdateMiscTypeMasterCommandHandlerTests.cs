using AutoMapper;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IMiscTypeMaster;
using LogisticsManagement.Application.MiscTypeMaster.Commands.UpdateMiscTypeMaster;
using LogisticsManagement.Domain.Events;
using LogisticsManagement.UnitTests.TestData;

namespace LogisticsManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int updatedId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<global::LogisticsManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<UpdateMiscTypeMasterCommand>()))
                .Returns(MiscTypeMasterBuilders.ValidEntity());

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<global::LogisticsManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(updatedId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Misc Type Master updated successfully.");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            SetupHappyPath(updatedId: 5);
            var result = await CreateSut().Handle(MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.Data.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<global::LogisticsManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "MISC_TYPE_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_UsesMapper()
        {
            SetupHappyPath();
            var command = MiscTypeMasterBuilders.ValidUpdateCommand();
            await CreateSut().Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<global::LogisticsManagement.Domain.Entities.MiscTypeMaster>(command),
                Times.Once);
        }
    }
}
