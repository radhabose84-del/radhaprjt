using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class CreateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = MiscMasterBuilders.ValidEntity(id);
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.MiscMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(entity);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<object>()))
                .Returns(MiscMasterBuilders.ValidDto(id));
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                MiscMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(MiscMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.MiscMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectId()
        {
            SetupHappyPath(42);
            var result = await CreateSut().Handle(MiscMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Id.Should().Be(42);
        }

        [Fact]
        public async Task Handle_FailedCreate_ThrowsException()
        {
            var entity = new PurchaseManagement.Domain.Entities.MiscMaster { Id = 0 };
            _mockMapper.Setup(m => m.Map<PurchaseManagement.Domain.Entities.MiscMaster>(It.IsAny<object>())).Returns(entity);
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.MiscMaster>())).ReturnsAsync(entity);

            Func<Task> act = async () => await CreateSut().Handle(
                MiscMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
