using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRules;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PutAway.Queries
{
    public sealed class GetPutAwayRulesQueryHandlerTests
    {
        private readonly Mock<IPutAwayRuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetPutAwayRulesQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var rows = new List<PutAwayRuleListDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((rows.AsEnumerable(), 1));
            _mockMapper.Setup(m => m.Map<List<PutAwayRuleListDto>>(It.IsAny<object>()))
                .Returns(new List<PutAwayRuleListDto> { new() { Id = 1 } });
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPutAwayRulesQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Enumerable.Empty<PutAwayRuleListDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<PutAwayRuleListDto>>(It.IsAny<object>()))
                .Returns(new List<PutAwayRuleListDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPutAwayRulesQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var rows = new List<PutAwayRuleListDto> { new() { Id = 1 }, new() { Id = 2 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "x", It.IsAny<CancellationToken>()))
                .ReturnsAsync((rows.AsEnumerable(), 12));
            _mockMapper.Setup(m => m.Map<List<PutAwayRuleListDto>>(It.IsAny<object>()))
                .Returns(rows);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPutAwayRulesQuery { PageNumber = 2, PageSize = 5, SearchTerm = "x" }, CancellationToken.None);

            result.TotalCount.Should().Be(12);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }
    }
}
