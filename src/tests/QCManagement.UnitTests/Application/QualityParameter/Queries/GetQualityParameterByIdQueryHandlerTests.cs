using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Dto;
using QCManagement.Application.QualityParameter.Queries.GetQualityParameterById;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualityParameter.Queries
{
    public class GetQualityParameterByIdQueryHandlerTests
    {
        private readonly Mock<IQualityParameterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        public GetQualityParameterByIdQueryHandlerTests()
        {
            _mockMapper.Setup(m => m.Map<QualityParameterDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as QualityParameterDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());
        }

        private GetQualityParameterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockUomLookup.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EntityExists_ReturnsNotNull()
        {
            var dto = QualityParameterBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetQualityParameterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var dto = QualityParameterBuilders.ValidDto(id: 1, code: "QP-000001");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetQualityParameterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.ParameterCode.Should().Be("QP-000001");
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((QualityParameterDto?)null);

            var result = await CreateSut().Handle(new GetQualityParameterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_EntityWithUom_PopulatesUomCodeAndName()
        {
            var dto = QualityParameterBuilders.ValidDto(id: 1, unitId: 12);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>
                {
                    new UOMLookupDto { Id = 12, Code = "N", UOMName = "Newton" }
                });

            var result = await CreateSut().Handle(new GetQualityParameterByIdQuery { Id = 1 }, CancellationToken.None);

            result!.UnitCode.Should().Be("N");
            result!.UnitName.Should().Be("Newton");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var dto = QualityParameterBuilders.ValidDto();
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            await CreateSut().Handle(new GetQualityParameterByIdQuery { Id = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
