using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupAutoComplete;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.FeederGroup.Queries
{
    public sealed class GetFeederGroupQueryHandlerTests
    {
        private readonly Mock<IFeederGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFeederGroupQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.Power.FeederGroup> { new() };
            var dtos = new List<FeederGroupDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllFeederGroupAsync(1, 10, null)).ReturnsAsync((entities, 1));
            _mockMapper.Setup(m => m.Map<List<FeederGroupDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetFeederGroupQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }

    public sealed class GetFeederGroupByIdQueryHandlerTests
    {
        private readonly Mock<IFeederGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFeederGroupByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetFeederGroupByIdAsync(1)).ReturnsAsync(new MaintenanceManagement.Domain.Entities.Power.FeederGroup());
            _mockMapper.Setup(m => m.Map<GetFeederGroupByIdDto>(It.IsAny<object>())).Returns(new GetFeederGroupByIdDto());

            var result = await CreateSut().Handle(new GetFeederGroupByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }
    }

    public sealed class GetFeederGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IFeederGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFeederGroupAutoCompleteQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            _mockQueryRepo.Setup(r => r.GetFeederGroupAutoComplete(It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.Power.FeederGroup> { new() });
            _mockMapper.Setup(m => m.Map<List<GetFeederGroupAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<GetFeederGroupAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(new GetFeederGroupAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
