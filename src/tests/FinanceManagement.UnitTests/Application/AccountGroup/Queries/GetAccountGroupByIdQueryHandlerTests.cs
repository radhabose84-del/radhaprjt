using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupById;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Queries
{
    public sealed class GetAccountGroupByIdQueryHandlerTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAccountGroupByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new AccountGroupDetailDto { Id = 1, GroupCode = "A-CA-INV-FF", Level = 4, IsLeaf = true };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetAccountGroupByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AccountGroupDetailDto?)null);

            var result = await CreateSut().Handle(new GetAccountGroupByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = new AccountGroupDetailDto { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            await CreateSut().Handle(new GetAccountGroupByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((AccountGroupDetailDto?)null);

            await CreateSut().Handle(new GetAccountGroupByIdQuery { Id = 999 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
