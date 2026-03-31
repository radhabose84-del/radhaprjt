using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class CreateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(id);
            var dto = MiscTypeMasterBuilders.ValidDto(id);

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<CreateMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);

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

            var result = await CreateSut().Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsMiscTypeMasterDto()
        {
            SetupHappyPath();

            var result = await CreateSut().Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().NotBeNull();
            result.Data!.MiscTypeCode.Should().Be("MT001");
        }

        [Fact]
        public async Task Handle_ZeroId_ReturnsFailure()
        {
            var entity = MiscTypeMasterBuilders.ValidEntity(0);

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<CreateMiscTypeMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            var result = await CreateSut().Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<InventoryManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
