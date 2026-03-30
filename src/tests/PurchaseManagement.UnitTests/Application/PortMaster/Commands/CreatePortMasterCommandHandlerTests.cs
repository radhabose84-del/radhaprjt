using AutoMapper;
using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
using PurchaseManagement.Application.Port.Commands;
using PurchaseManagement.Application.Port.Dto;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PortMaster.Commands
{
    public sealed class CreatePortMasterCommandHandlerTests
    {
        private readonly Mock<IPortMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IPortMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreatePortMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = PortMasterBuilders.ValidEntity(id);
            var dto = PortMasterBuilders.ValidDto(id);

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.PortMaster>(It.IsAny<CreatePortMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.PortMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<PortMasterDto>(It.IsAny<PortMasterDto>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath();
            var command = PortMasterBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.PortCode.Should().Be("PORT001");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(PortMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.PortMaster>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(PortMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ZeroId_ThrowsExceptionRules()
        {
            var entity = PortMasterBuilders.ValidEntity(0); // Id = 0 -> throw
            var dto = PortMasterBuilders.ValidDto(0);

            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.PortMaster>(It.IsAny<CreatePortMasterCommand>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.PortMaster>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(0, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            _mockMapper
                .Setup(m => m.Map<PortMasterDto>(It.IsAny<PortMasterDto>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(PortMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
