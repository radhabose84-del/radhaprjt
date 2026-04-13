using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceRequestById;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Queries.BatchD
{
    public sealed class GetMaintenanceRequestByIdQueryHandlerBatchDTests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMaintenanceRequestByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WhenRecordNotFound_ReturnsFailure()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((object?)null);

            var result = await CreateSut().Handle(new GetMaintenanceRequestByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_WhenRecordFound_ReturnsSuccess()
        {
            dynamic dto = new System.Dynamic.ExpandoObject();
            dto.Id = 1;
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((object)dto);
            _mockMapper.Setup(m => m.Map<GetMaintenanceRequestDto>(It.IsAny<object>()))
                .Returns(new GetMaintenanceRequestDto { Id = 1 });

            var result = await CreateSut().Handle(new GetMaintenanceRequestByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }
    }
}
