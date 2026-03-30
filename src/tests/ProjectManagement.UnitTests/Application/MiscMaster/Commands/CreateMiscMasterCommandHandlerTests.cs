using AutoMapper;
using Contracts.Common;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.CreateMiscMaster;
using ProjectManagement.Application.MiscMaster.Queries.GetMiscMaster;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.MiscMaster.Commands
{
    public sealed class CreateMiscMasterCommandHandlerTests
    {
        private readonly Mock<IMiscMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateMiscMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(CreateMiscMasterCommand command, int newId = 1)
        {
            var entity = MiscMasterBuilders.ValidEntity(id: newId);
            var dto = MiscMasterBuilders.ValidDto(id: newId);

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<ProjectManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(newId))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(entity))
                .Returns(dto);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsDto()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 5);
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<ProjectManagement.Domain.Entities.MiscMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZeroId_ThrowsException()
        {
            var command = MiscMasterBuilders.ValidCreateCommand();
            var entity = MiscMasterBuilders.ValidEntity(id: 0);
            var dto = MiscMasterBuilders.ValidDto(id: 0);

            _mockMapper
                .Setup(m => m.Map<ProjectManagement.Domain.Entities.MiscMaster>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<ProjectManagement.Domain.Entities.MiscMaster>()))
                .ReturnsAsync(entity);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(0))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetMiscMasterDto>(entity))
                .Returns(dto);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
