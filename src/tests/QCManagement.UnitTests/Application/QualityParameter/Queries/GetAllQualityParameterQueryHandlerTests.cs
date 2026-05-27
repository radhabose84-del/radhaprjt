using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityParameter;
using QCManagement.Application.QualityParameter.Dto;
using QCManagement.Application.QualityParameter.Queries.GetAllQualityParameter;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualityParameter.Queries
{
    public class GetAllQualityParameterQueryHandlerTests
    {
        private readonly Mock<IQualityParameterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        public GetAllQualityParameterQueryHandlerTests()
        {
            _mockMapper.Setup(m => m.Map<List<QualityParameterDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<QualityParameterDto> ?? new List<QualityParameterDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>());
        }

        private GetAllQualityParameterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockUomLookup.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<QualityParameterDto> { QualityParameterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null, null)).ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllQualityParameterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData()
        {
            var dtoList = new List<QualityParameterDto> { QualityParameterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null, null)).ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllQualityParameterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Data!.Should().HaveCount(1);
            result.Data![0].ParameterCode.Should().Be("QP-000001");
        }

        [Fact]
        public async Task Handle_PopulatesUomCodeAndName_FromLookup()
        {
            var dto = QualityParameterBuilders.ValidDto(unitId: 12);
            var dtoList = new List<QualityParameterDto> { dto };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null, null)).ReturnsAsync((dtoList, 1));
            _mockUomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>
                {
                    new UOMLookupDto { Id = 12, Code = "N", UOMName = "Newton" }
                });

            var result = await CreateSut().Handle(
                new GetAllQualityParameterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Data![0].UnitCode.Should().Be("N");
            result.Data![0].UnitName.Should().Be("Newton");
        }

        [Fact]
        public async Task Handle_FilterByParameterGroupId_PassesToRepo()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, 5))
                .ReturnsAsync((new List<QualityParameterDto>(), 0));

            await CreateSut().Handle(
                new GetAllQualityParameterQuery { PageNumber = 1, PageSize = 10, ParameterGroupId = 5 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, null, 5), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null, null))
                .ReturnsAsync((new List<QualityParameterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllQualityParameterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
