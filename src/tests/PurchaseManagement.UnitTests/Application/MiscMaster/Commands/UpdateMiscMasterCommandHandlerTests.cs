using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class UpdateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(bool updateResult = true)
        {
            var entity = MiscMasterBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.MiscMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PurchaseManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(updateResult);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(
                MiscMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(MiscMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PurchaseManagement.Domain.Entities.MiscMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateFailed_ThrowsException()
        {
            SetupHappyPath(false);

            Func<Task> act = async () => await CreateSut().Handle(
                MiscMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
