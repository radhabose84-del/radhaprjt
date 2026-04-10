using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using GateEntryManagement.Application.MiscTypeMaster.Dto;
using GateEntryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var lookupList = new List<MiscTypeMasterLookupDto>
            {
                new MiscTypeMasterLookupDto { Id = 1, MiscTypeCode = "MT001", Description = "Test" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery("Test"),
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].MiscTypeCode.Should().Be("MT001");
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsResults()
        {
            var lookupList = new List<MiscTypeMasterLookupDto>
            {
                new MiscTypeMasterLookupDto { Id = 1, MiscTypeCode = "MT001", Description = "Test" },
                new MiscTypeMasterLookupDto { Id = 2, MiscTypeCode = "MT002", Description = "Test2" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery(null!),
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MiscTypeMasterLookupDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery("Test"),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.ActionCode == "GetMiscTypeMasterAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
