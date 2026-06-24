using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IOutbox;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Domain.Events;
using PartyManagement.UnitTests.TestData;
using Xunit;

namespace PartyManagement.UnitTests.Application.PartyMaster.Commands
{
    public sealed class CreatePartyMasterCommandHandlerTests
    {
        private readonly Mock<IPartyMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IPartyActivityLogCommandRepository> _mockActivityLog = new(MockBehavior.Loose);
        private readonly Mock<ILocationLookup> _mockLocationLookup = new(MockBehavior.Loose);
        private readonly Mock<ILocationMasterLookup> _mockLocationMasterLookup = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);

        private CreatePartyMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockQueryRepo.Object, _mockActivityLog.Object,
                _mockLocationLookup.Object, _mockLocationMasterLookup.Object, _mockOutbox.Object);

        private void SetupHappyPath(int newId = 1)
        {
            var entity = PartyMasterBuilders.ValidEntity(newId);
            var workflowDto = new PartyMasterWorkFlowDto { Id = newId, PartyCode = "PAR001", PartyName = "Test Party" };

            _mockCommandRepo.Setup(r => r.GetNextPartyCodeAsync()).ReturnsAsync("PAR001");
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<PartyManagement.Domain.Entities.PartyMaster>())).ReturnsAsync(newId);
            _mockCommandRepo.Setup(r => r.GetByIdPartyMasterWorkFlowAsync(newId)).ReturnsAsync(workflowDto);

            _mockMapper
                .Setup(m => m.Map<PartyManagement.Domain.Entities.PartyMaster>(It.IsAny<object>()))
                .Returns(entity);

            _mockActivityLog
                .Setup(r => r.InsertAsync(It.IsAny<PartyManagement.Domain.Entities.PartyActivityLog>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(5);
            var command = PartyMasterBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(5);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsGreaterThanZero()
        {
            SetupHappyPath(1);
            var command = PartyMasterBuilders.ValidCreateCommand();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath(1);
            var command = PartyMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<PartyManagement.Domain.Entities.PartyMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath(1);
            var command = PartyMasterBuilders.ValidCreateCommand();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateReturnsZero_ThrowsExceptionRules()
        {
            var entity = PartyMasterBuilders.ValidEntity(1);
            var workflowDto = new PartyMasterWorkFlowDto { Id = 0 };

            _mockCommandRepo.Setup(r => r.GetNextPartyCodeAsync()).ReturnsAsync("PAR001");
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<PartyManagement.Domain.Entities.PartyMaster>())).ReturnsAsync(0);
            _mockMapper.Setup(m => m.Map<PartyManagement.Domain.Entities.PartyMaster>(It.IsAny<object>())).Returns(entity);

            var command = PartyMasterBuilders.ValidCreateCommand();

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<Contracts.Common.ExceptionRules>();
        }
    }
}
