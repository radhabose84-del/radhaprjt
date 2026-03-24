using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(bool deleteResult = true)
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<PurchaseManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(deleteResult);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(
                MiscTypeMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsDeleteOnce()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(MiscTypeMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<PurchaseManagement.Domain.Entities.MiscTypeMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFailed_ReturnsFailure()
        {
            SetupHappyPath(false);
            var result = await CreateSut().Handle(
                MiscTypeMasterBuilders.ValidDeleteCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
