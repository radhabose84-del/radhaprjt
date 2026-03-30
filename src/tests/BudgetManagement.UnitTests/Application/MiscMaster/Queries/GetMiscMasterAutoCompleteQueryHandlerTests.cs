using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMaster;
using BudgetManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<BudgetManagement.Domain.Entities.MiscMaster> { MiscMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscMasterAutoCompleteDto> { MiscMasterBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo.Setup(r => r.GetMiscMaster("test", "TypeCode")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(entities)).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = "test", MiscTypeCode = "TypeCode" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullSearchPattern_UsesEmptyString()
        {
            var entities = new List<BudgetManagement.Domain.Entities.MiscMaster> { MiscMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscMasterAutoCompleteDto> { MiscMasterBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo.Setup(r => r.GetMiscMaster(string.Empty, string.Empty)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(entities)).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscMasterAutoCompleteQuery { SearchPattern = null, MiscTypeCode = null },
                CancellationToken.None);

            result.Should().NotBeNull();
        }
    }
}
