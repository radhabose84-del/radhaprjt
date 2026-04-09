using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public sealed class GetMiscTypeMasterQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMiscTypeMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupGetAll(int page, int size, string? search, List<MaintenanceManagement.Domain.Entities.MiscTypeMaster> entities, int total)
        {
            _mockQueryRepo.Setup(r => r.GetAllMiscTypeMasterAsync(page, size, search)).ReturnsAsync((entities, total));
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MiscTypeMaster> { new() };
            var dtos = new List<GetMiscTypeMasterDto> { new() { Id = 1 } };
            SetupGetAll(1, 10, null, entities, 1);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsPaginationMetadata()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MiscTypeMaster> { new() };
            var dtos = new List<GetMiscTypeMasterDto> { new() };
            SetupGetAll(2, 5, "search", entities, 11);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetMiscTypeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            SetupGetAll(1, 10, null, new(), 0);
            _mockMapper.Setup(m => m.Map<List<GetMiscTypeMasterDto>>(It.IsAny<object>())).Returns(new List<GetMiscTypeMasterDto>());

            var result = await CreateSut().Handle(new GetMiscTypeMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
