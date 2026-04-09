using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeeder;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederAutoComplete;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Feeder.Queries
{
    public sealed class GetFeederQueryHandlerTests
    {
        private readonly Mock<IFeederQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetFeederQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object, _mockUnitLookup.Object);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var dtos = new List<GetFeederDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllFeederAsync(1, 10, null)).ReturnsAsync((dtos, 1));
            _mockMapper.Setup(m => m.Map<List<GetFeederDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetFeederQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllFeederAsync(1, 10, null)).ReturnsAsync((new List<GetFeederDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<GetFeederDto>>(It.IsAny<object>())).Returns(new List<GetFeederDto>());

            var result = await CreateSut().Handle(new GetFeederQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }

    public sealed class GetFeederByIdQueryHandlerTests
    {
        private readonly Mock<IFeederQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFeederByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetFeederByIdAsync(1)).ReturnsAsync(new MaintenanceManagement.Domain.Entities.Power.Feeder());
            _mockMapper.Setup(m => m.Map<GetFeederByIdDto>(It.IsAny<object>())).Returns(new GetFeederByIdDto());

            var result = await CreateSut().Handle(new GetFeederByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }
    }

    public sealed class GetFeederAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IFeederQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetFeederAutoCompleteQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            _mockQueryRepo.Setup(r => r.GetFeederAutoComplete(It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.Power.Feeder> { new() });
            _mockMapper.Setup(m => m.Map<List<GetFeederAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<GetFeederAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(new GetFeederAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
