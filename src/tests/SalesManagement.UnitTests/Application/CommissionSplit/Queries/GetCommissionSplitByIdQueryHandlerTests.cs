using AutoMapper;
using MediatR;
using SalesManagement.Domain.Events;
using SalesManagement.Application.Common.Interfaces.ICommissionSplit;
using SalesManagement.Application.CommissionSplit.Dto;
using SalesManagement.Application.CommissionSplit.Queries.GetCommissionSplitById;

namespace SalesManagement.UnitTests.Application.CommissionSplit.Queries
{
    public sealed class GetCommissionSplitByIdQueryHandlerTests
    {
        private readonly Mock<ICommissionSplitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCommissionSplitByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new CommissionSplitDto { Id = 5 });

            var result = await CreateSut().Handle(new GetCommissionSplitByIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
        }

        [Fact]
        public async Task Handle_MissingId_ReturnsNull_AndDoesNotPublish()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CommissionSplitDto?)null);

            var result = await CreateSut().Handle(new GetCommissionSplitByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(new CommissionSplitDto { Id = 5 });

            await CreateSut().Handle(new GetCommissionSplitByIdQuery { Id = 5 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
