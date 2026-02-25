#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;
using SalesManagement.Application.SalesGroup.Queries.GetSalesGroupById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesGroup.Queries
{
    public class GetSalesGroupByIdQueryHandlerTests
    {
        private readonly Mock<ISalesGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetSalesGroupByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<SalesGroupDto>(It.IsAny<object>()))
                .Returns<object>(o => o as SalesGroupDto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetSalesGroupByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // â”€â”€ Tests â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        [Fact]
        public async Task Handle_EntityExists_ReturnsNotNull()
        {
            var query = new GetSalesGroupByIdQuery { Id = 1 };
            var dto = SalesGroupBuilders.ValidDto(id: 1, name: "Test Sales Group");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var query = new GetSalesGroupByIdQuery { Id = 1 };
            var dto = SalesGroupBuilders.ValidDto(id: 1, name: "North Region Group");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.SalesGroupName.Should().Be("North Region Group");
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditEvent()
        {
            var query = new GetSalesGroupByIdQuery { Id = 1 };
            var dto = SalesGroupBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            var sut = CreateSut();

            await sut.Handle(query, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var query = new GetSalesGroupByIdQuery { Id = 7 };
            var dto = SalesGroupBuilders.ValidDto(id: 7);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);
            var sut = CreateSut();

            await sut.Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            var query = new GetSalesGroupByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesGroupDto)null);
            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}