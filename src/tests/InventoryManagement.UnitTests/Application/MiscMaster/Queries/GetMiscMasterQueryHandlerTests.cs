using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMaster;
using InventoryManagement.UnitTests.TestData;
using MediatR;

namespace InventoryManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterQueryHanlder CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(1, 15, null))
                .ReturnsAsync((new List<InventoryManagement.Domain.Entities.MiscMaster>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<List<InventoryManagement.Domain.Entities.MiscMaster>>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsCorrectCount()
        {
            var entities = new List<InventoryManagement.Domain.Entities.MiscMaster>
            {
                MiscMasterBuilders.ValidEntity(1),
                MiscMasterBuilders.ValidEntity(2)
            };
            var dtos = new List<GetMiscMasterDto>
            {
                MiscMasterBuilders.ValidDto(1),
                MiscMasterBuilders.ValidDto(2)
            };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(1, 15, null))
                .ReturnsAsync((entities, 2));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<List<InventoryManagement.Domain.Entities.MiscMaster>>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Data!.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(2, 10, "search"))
                .ReturnsAsync((new List<InventoryManagement.Domain.Entities.MiscMaster>(), 11));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<List<InventoryManagement.Domain.Entities.MiscMaster>>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 2, PageSize = 10, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(10);
            result.TotalCount.Should().Be(11);
        }
    }
}
