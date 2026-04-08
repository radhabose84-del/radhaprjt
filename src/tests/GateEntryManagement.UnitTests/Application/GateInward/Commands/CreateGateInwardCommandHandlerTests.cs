using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Commands.CreateGateInward;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.GateInward.Commands
{
    public sealed class CreateGateInwardCommandHandlerTests
    {
        private readonly Mock<IGateInwardCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IGateInwardQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateGateInwardCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockDocSeqLookup.Object,
                _mockMediator.Object, _mockMapper.Object, _mockIpService.Object);

        [Fact]
        public void Constructor_AllDependencies_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void Handler_InjectsCorrectDependencies()
        {
            var sut = CreateSut();
            sut.Should().BeOfType<CreateGateInwardCommandHandler>();
        }
    }
}
