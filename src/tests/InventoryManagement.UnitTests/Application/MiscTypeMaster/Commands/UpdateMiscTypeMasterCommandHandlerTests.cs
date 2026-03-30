using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class UpdateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity();

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync((InventoryManagement.Domain.Entities.MiscTypeMaster?)null);

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<UpdateMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();

            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MT001", Description = "Updated", IsActive = 1 };
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();

            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MT001", Description = "Updated", IsActive = 1 };
            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MT001", Description = "Updated", IsActive = 1 };
            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateCode_ReturnsFailure()
        {
            var existing = MiscTypeMasterBuilders.ValidEntity(2);

            _mockQueryRepo
                .Setup(r => r.GetByMiscTypeMasterCodeAsync(It.IsAny<string?>(), It.IsAny<int?>()))
                .ReturnsAsync(existing);

            var command = new UpdateMiscTypeMasterCommand { Id = 1, MiscTypeCode = "MT001", Description = "Updated", IsActive = 1 };
            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
