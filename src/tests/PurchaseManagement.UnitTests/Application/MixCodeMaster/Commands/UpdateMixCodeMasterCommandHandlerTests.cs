using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Commands.UpdateMixCodeMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MixCodeMaster.Commands
{
    public sealed class UpdateMixCodeMasterCommandHandlerTests
    {
        private readonly Mock<IMixCodeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMixCodeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateMixCodeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.MixCodeMaster>(It.IsAny<object>()))
                .Returns(MixCodeMasterBuilders.ValidEntity(id));
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.MixCodeMaster>()))
                .ReturnsAsync(id);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(MixCodeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(MixCodeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.MixCodeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(MixCodeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InactivateWhileLinked_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.IsMixCodeMasterLinkedAsync(1))
                .ReturnsAsync(true);

            var command = MixCodeMasterBuilders.ValidUpdateCommand(id: 1, isActive: 0);
            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*inactivate*");
        }

        [Fact]
        public async Task Handle_InactivateWhileLinked_DoesNotUpdate()
        {
            _mockQueryRepo
                .Setup(r => r.IsMixCodeMasterLinkedAsync(1))
                .ReturnsAsync(true);

            var command = MixCodeMasterBuilders.ValidUpdateCommand(id: 1, isActive: 0);
            try { await CreateSut().Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<PurchaseManagement.Domain.Entities.MixCodeMaster>()),
                Times.Never);
        }
    }
}
