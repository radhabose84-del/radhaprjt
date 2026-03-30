using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.MiscTypeMaster.Commands
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
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(entity);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<object>()))
                .Returns(MiscTypeMasterBuilders.ValidDto(id));
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.MiscTypeMaster>()), Times.Once);
        }

        [Fact]
        public async Task Handle_FailedCreate_ReturnsFailure()
        {
            var entity = new PurchaseManagement.Domain.Entities.MiscTypeMaster { Id = 0 };
            _mockMapper.Setup(m => m.Map<PurchaseManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>())).Returns(entity);
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.MiscTypeMaster>())).ReturnsAsync(entity);

            var result = await CreateSut().Handle(MiscTypeMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
