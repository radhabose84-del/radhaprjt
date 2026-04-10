using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMasterAutoComplete;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetMiscMasterByIdQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MaintenanceManagement.Domain.Entities.MiscMaster { Id = 1 });
            _mockMapper.Setup(m => m.Map<GetMiscMasterDto>(It.IsAny<object>())).Returns(new GetMiscMasterDto());

            var result = await CreateSut().Handle(new GetMiscMasterByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }
    }

    public sealed class GetMiscMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscMasterAutoCompleteQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            _mockQueryRepo.Setup(r => r.GetMiscMaster(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MiscMaster> { new() });
            _mockMapper.Setup(m => m.Map<List<GetMiscMasterAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<GetMiscMasterAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(new GetMiscMasterAutoCompleteQuery { SearchPattern = "test", MiscTypeCode = "MT01" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
