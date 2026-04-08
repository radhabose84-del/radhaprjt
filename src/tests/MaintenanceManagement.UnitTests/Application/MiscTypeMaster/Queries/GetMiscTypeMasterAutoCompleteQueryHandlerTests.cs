using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterAutoComplete;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MiscTypeMaster> { new() };
            var dtos = new List<GetMiscTypeMasterAutocompleteDto> { new() };
            _mockQueryRepo.Setup(r => r.GetMiscTypeMaster(It.IsAny<string>())).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterAutocompleteDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "Test" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailureWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetMiscTypeMaster(It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscTypeMaster>());

            var result = await CreateSut().Handle(new GetMiscTypeMasterAutoCompleteQuery { SearchPattern = "NONE" }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeEmpty();
        }
    }
}
