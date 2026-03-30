using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MaintenanceManagement.Domain.Events;
using MaintenanceManagement.UnitTests.TestData;
using MediatR;
using Xunit;

namespace MaintenanceManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class CreateMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = MiscTypeMasterMMBuilders.ValidEntity(newId);
            var dto = MiscTypeMasterMMBuilders.ValidDto(newId);

            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(newId))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscTypeMasterDto>(It.IsAny<object>()))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(1);
            var result = await CreateSut().Handle(
                MiscTypeMasterMMBuilders.ValidCreateCommand(), CancellationToken.None);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                MiscTypeMasterMMBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MiscTypeMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            await CreateSut().Handle(
                MiscTypeMasterMMBuilders.ValidCreateCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_ReturnsFailure()
        {
            var zeroEntity = MiscTypeMasterMMBuilders.ValidEntity(0);

            _mockMapper
                .Setup(m => m.Map<MaintenanceManagement.Domain.Entities.MiscTypeMaster>(It.IsAny<object>()))
                .Returns(zeroEntity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<MaintenanceManagement.Domain.Entities.MiscTypeMaster>()))
                .ReturnsAsync(zeroEntity);

            var result = await CreateSut().Handle(
                MiscTypeMasterMMBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
