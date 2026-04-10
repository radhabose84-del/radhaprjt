using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetail;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ShiftMasterDetail.Queries
{
    public sealed class GetShiftMasterDetailQueryHandlerTests
    {
        private readonly Mock<IShiftMasterDetailQuery> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetShiftMasterDetailQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<dynamic> { new { Id = 1 } };
            var dtos = new List<ShiftMasterDetailDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllShiftMasterDetailAsync(1, 10, null)).ReturnsAsync(((IEnumerable<dynamic>)entities, 1));
            _mockMapper.Setup(m => m.Map<List<ShiftMasterDetailDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetShiftMasterDetailQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllShiftMasterDetailAsync(1, 10, null)).ReturnsAsync(((IEnumerable<dynamic>)new List<dynamic>(), 0));
            _mockMapper.Setup(m => m.Map<List<ShiftMasterDetailDto>>(It.IsAny<object>())).Returns(new List<ShiftMasterDetailDto>());

            var result = await CreateSut().Handle(new GetShiftMasterDetailQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
