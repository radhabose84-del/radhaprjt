using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;
using SalesManagement.Application.SalesChannel.Queries.GetSalesChannelById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesChannel.Queries
{
    public class GetSalesChannelByIdQueryHandlerTests
    {
        private readonly Mock<ISalesChannelQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetSalesChannelByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<SalesChannelDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as SalesChannelDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetSalesChannelByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // â”€â”€ Tests â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        [Fact]
        public async Task Handle_EntityExists_ReturnsNotNull()
        {
            var query = new GetSalesChannelByIdQuery { Id = 1 };
            var dto = SalesChannelBuilders.ValidDto(id: 1, code: "CH001");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var query = new GetSalesChannelByIdQuery { Id = 1 };
            var dto = SalesChannelBuilders.ValidDto(id: 1, code: "CH001", name: "Test Channel");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.SalesChannelCode.Should().Be("CH001");
            result!.SalesChannelName.Should().Be("Test Channel");
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditEvent()
        {
            var query = new GetSalesChannelByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(SalesChannelBuilders.ValidDto(id: 1));

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var query = new GetSalesChannelByIdQuery { Id = 7 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(SalesChannelBuilders.ValidDto(id: 7));

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            var query = new GetSalesChannelByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesChannelDto?)null);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}