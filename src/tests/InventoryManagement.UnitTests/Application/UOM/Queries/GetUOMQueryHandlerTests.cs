using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.Domain.Events;

namespace InventoryManagement.UnitTests.Application.UOM.Queries
{
    public sealed class GetUOMQueryHandlerTests
    {
        private readonly Mock<IUOMQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUOMHandlerQuery CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyList = new List<InventoryManagement.Domain.Entities.UOM>();
            _mockQueryRepo
                .Setup(r => r.GetAllUOMAsync(1, 15, null))
                .ReturnsAsync((emptyList, 0));
            _mockMapper
                .Setup(m => m.Map<List<UOMDto>>(It.IsAny<object>()))
                .Returns(new List<UOMDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUOMQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<InventoryManagement.Domain.Entities.UOM>();
            _mockQueryRepo
                .Setup(r => r.GetAllUOMAsync(2, 5, "KG"))
                .ReturnsAsync((entities, 11));
            _mockMapper
                .Setup(m => m.Map<List<UOMDto>>(It.IsAny<object>()))
                .Returns(new List<UOMDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUOMQuery { PageNumber = 2, PageSize = 5, SearchTerm = "KG" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
