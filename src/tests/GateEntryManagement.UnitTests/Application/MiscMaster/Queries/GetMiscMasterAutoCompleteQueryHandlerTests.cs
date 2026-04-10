using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.MiscMaster.Dto;
using GateEntryManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var lookupList = new List<MiscMasterLookupDto>
            {
                new MiscMasterLookupDto { Id = 1, Code = "MM001", Description = "Test" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Test", "GATE", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("Test", "GATE"),
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("MM001");
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsResults()
        {
            var lookupList = new List<MiscMasterLookupDto>
            {
                new MiscMasterLookupDto { Id = 1, Code = "MM001", Description = "Test" },
                new MiscMasterLookupDto { Id = 2, Code = "MM002", Description = "Test2" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("", "GATE", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery(null!, "GATE"),
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Test", "GATE", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MiscMasterLookupDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery("Test", "GATE"),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.ActionCode == "GetMiscMasterAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
