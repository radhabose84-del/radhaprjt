using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class DeleteMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(bool deleteResult = true)
        {
            var entity = MiscMasterBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.MiscMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<PurchaseManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(deleteResult);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(
                MiscMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(MiscMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<PurchaseManagement.Domain.Entities.MiscMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFailed_ThrowsException()
        {
            SetupHappyPath(false);

            Func<Task> act = async () => await CreateSut().Handle(
                MiscMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
