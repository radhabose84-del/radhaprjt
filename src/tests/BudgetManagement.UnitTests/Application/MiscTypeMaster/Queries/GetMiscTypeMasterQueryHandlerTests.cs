using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BudgetManagement.Domain.Events;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<BudgetManagement.Domain.Entities.MiscTypeMaster> { MiscTypeMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<List<BudgetManagement.Domain.Entities.MiscTypeMaster>>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<BudgetManagement.Domain.Entities.MiscTypeMaster> { MiscTypeMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(2, 5, "search"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<List<BudgetManagement.Domain.Entities.MiscTypeMaster>>()))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllMiscTypeMasterAsync(1, 10, null))
                .ReturnsAsync((new List<BudgetManagement.Domain.Entities.MiscTypeMaster>(), 0));

            _mockMapper
                .Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<List<BudgetManagement.Domain.Entities.MiscTypeMaster>>()))
                .Returns(new List<GetMiscTypeMasterDto>());

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
