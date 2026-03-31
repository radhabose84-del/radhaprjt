using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 15, null))
                .ReturnsAsync((new List<InventoryManagement.Domain.Entities.MiscTypeMaster>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscTypeMasterDto>());

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsCorrectCount()
        {
            var entities = new List<InventoryManagement.Domain.Entities.MiscTypeMaster>
            {
                MiscTypeMasterBuilders.ValidEntity(1),
                MiscTypeMasterBuilders.ValidEntity(2)
            };
            var dtos = new List<GetMiscTypeMasterDto>
            {
                MiscTypeMasterBuilders.ValidDto(1),
                MiscTypeMasterBuilders.ValidDto(2)
            };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 10, null))
                .ReturnsAsync((entities, 2));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<object>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Data!.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(2, 10, "test"))
                .ReturnsAsync((new List<InventoryManagement.Domain.Entities.MiscTypeMaster>(), 11));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetMiscTypeMasterDto>());

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 2, PageSize = 10, SearchTerm = "test" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(11);
        }
    }
}
