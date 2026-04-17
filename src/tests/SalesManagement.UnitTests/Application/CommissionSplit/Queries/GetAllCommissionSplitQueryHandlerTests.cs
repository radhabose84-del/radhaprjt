using AutoMapper;
using MediatR;
using SalesManagement.Domain.Events;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Dto;
using SalesManagement.Application.CommissionSplit.Queries.GetAllCommissionSplit;

namespace SalesManagement.UnitTests.Application.CommissionSplit.Queries
{
    public sealed class GetAllCommissionSplitQueryHandlerTests
    {
        private readonly Mock<ICommissionSplitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllCommissionSplitQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<CommissionSplitDto>(), 0));

            var result = await CreateSut().Handle(new GetAllCommissionSplitQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_PropagatesPagination()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "x"))
                .ReturnsAsync((new List<CommissionSplitDto> { new() }, 17));

            var result = await CreateSut().Handle(
                new GetAllCommissionSplitQuery { PageNumber = 2, PageSize = 5, SearchTerm = "x" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(17);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<CommissionSplitDto>(), 0));

            await CreateSut().Handle(new GetAllCommissionSplitQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
