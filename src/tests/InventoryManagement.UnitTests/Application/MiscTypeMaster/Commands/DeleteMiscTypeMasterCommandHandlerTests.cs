using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(id);
            var dto = MiscTypeMasterBuilders.ValidDto(id);

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<DeleteMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(true);

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(new DeleteMiscTypeMasterCommand { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsDeleteOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(new DeleteMiscTypeMasterCommand { Id = 1 }, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(new DeleteMiscTypeMasterCommand { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFails_ReturnsFailure()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(1);

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<DeleteMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(false);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new DeleteMiscTypeMasterCommand { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
