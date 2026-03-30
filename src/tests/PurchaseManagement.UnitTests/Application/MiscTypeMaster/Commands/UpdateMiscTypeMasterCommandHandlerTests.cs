using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(bool updateResult = true)
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(1);
            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((PurchaseManagement.Domain.Entities.MiscTypeMaster?)null);
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PurchaseManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(updateResult);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(true);
            var result = await CreateSut().Handle(
                MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath(true);
            await CreateSut().Handle(MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<PurchaseManagement.Domain.Entities.MiscTypeMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UpdateFailed_ReturnsFailure()
        {
            SetupHappyPath(false);
            var result = await CreateSut().Handle(
                MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ExistingCode_ReturnsFailure()
        {
            var existing = MiscTypeMasterBuilders.ValidEntity(2);
            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(existing);

            var result = await CreateSut().Handle(
                MiscTypeMasterBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
