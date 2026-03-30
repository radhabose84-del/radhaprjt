using AutoMapper;
using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using BudgetManagement.UnitTests.TestData;
using MediatR;

namespace BudgetManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<BudgetManagement.Domain.Entities.MiscTypeMaster> { MiscTypeMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto> { MiscTypeMasterBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo.Setup(r => r.GetMiscTypeMaster("test")).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(entities)).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetMiscTypeMaster("xyz"))
                .ReturnsAsync(new List<BudgetManagement.Domain.Entities.MiscTypeMaster>());

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "xyz" },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NullSearchPattern_UsesEmptyString()
        {
            var entities = new List<BudgetManagement.Domain.Entities.MiscTypeMaster> { MiscTypeMasterBuilders.ValidEntity() };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto> { MiscTypeMasterBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo.Setup(r => r.GetMiscTypeMaster(string.Empty)).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(entities)).Returns(dtos);

            var result = await CreateSut().Handle(
                new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
