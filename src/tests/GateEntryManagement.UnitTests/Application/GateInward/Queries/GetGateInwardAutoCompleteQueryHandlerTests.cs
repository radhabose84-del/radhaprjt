using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Application.GateInward.Queries.GetGateInwardAutoComplete;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.GateInward.Queries
{
    public sealed class GetGateInwardAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IGateInwardQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetGateInwardAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var lookupList = new List<GateInwardAutoCompleteDto>
            {
                new GateInwardAutoCompleteDto { Id = 1, GateEntryNo = "GE001", VehicleNumber = "KA01AB1234" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("GE001", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetGateInwardAutoCompleteQuery("GE001"),
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].GateEntryNo.Should().Be("GE001");
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsResults()
        {
            var lookupList = new List<GateInwardAutoCompleteDto>
            {
                new GateInwardAutoCompleteDto { Id = 1, GateEntryNo = "GE001" },
                new GateInwardAutoCompleteDto { Id = 2, GateEntryNo = "GE002" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetGateInwardAutoCompleteQuery(null!),
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GateInwardAutoCompleteDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetGateInwardAutoCompleteQuery("Test"),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.ActionCode == "GetGateInwardAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
