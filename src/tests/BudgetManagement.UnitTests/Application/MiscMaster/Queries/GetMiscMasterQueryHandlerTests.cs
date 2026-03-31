using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterQueryHanlder CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<BudgetManagement.Domain.Entities.MiscMaster> { MiscMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscMasterDto> { MiscMasterBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<List<BudgetManagement.Domain.Entities.MiscMaster>>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<BudgetManagement.Domain.Entities.MiscMaster> { MiscMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscMasterDto> { MiscMasterBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(2, 5, "test"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<List<BudgetManagement.Domain.Entities.MiscMaster>>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscMasterAsync(1, 10, null))
                .ReturnsAsync((new List<BudgetManagement.Domain.Entities.MiscMaster>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscMasterDto>>(It.IsAny<List<BudgetManagement.Domain.Entities.MiscMaster>>()))
                .Returns(new List<GetMiscMasterDto>());

            var result = await CreateSut().Handle(
                new GetMiscMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
