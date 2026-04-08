using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Application.StoReceipt.Queries.GetStoReceiptById;

namespace SalesManagement.UnitTests.Application.StoReceipt.Queries
{
    public sealed class GetStoReceiptByIdQueryHandlerTests
    {
        private readonly Mock<IStoReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetStoReceiptByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new StoReceiptHeaderDto { Id = 1 });

            var result = await CreateSut().Handle(new GetStoReceiptByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((StoReceiptHeaderDto?)null);

            var result = await CreateSut().Handle(new GetStoReceiptByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
