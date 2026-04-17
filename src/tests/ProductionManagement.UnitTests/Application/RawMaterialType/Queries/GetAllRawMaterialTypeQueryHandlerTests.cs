using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Dto;
using ProductionManagement.Application.RawMaterialType.Queries.GetAllRawMaterialType;

namespace ProductionManagement.UnitTests.Application.RawMaterialType.Queries
{
    public sealed class GetAllRawMaterialTypeQueryHandlerTests
    {
        private readonly Mock<IRawMaterialTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllRawMaterialTypeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess_WithMappedData()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<RawMaterialTypeDto>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<RawMaterialTypeDto>>(It.IsAny<List<RawMaterialTypeDto>>()))
                .Returns(new List<RawMaterialTypeDto>());

            var result = await CreateSut().Handle(
                new GetAllRawMaterialTypeQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_PropagatesPaginationMetadata()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search"))
                .ReturnsAsync((new List<RawMaterialTypeDto>(), 23));
            _mockMapper
                .Setup(m => m.Map<List<RawMaterialTypeDto>>(It.IsAny<List<RawMaterialTypeDto>>()))
                .Returns(new List<RawMaterialTypeDto>());

            var result = await CreateSut().Handle(
                new GetAllRawMaterialTypeQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(23);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<RawMaterialTypeDto>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<RawMaterialTypeDto>>(It.IsAny<List<RawMaterialTypeDto>>()))
                .Returns(new List<RawMaterialTypeDto>());

            await CreateSut().Handle(new GetAllRawMaterialTypeQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
