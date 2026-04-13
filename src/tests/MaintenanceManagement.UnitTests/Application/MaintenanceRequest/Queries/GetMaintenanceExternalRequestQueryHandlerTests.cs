using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetMaintenanceExternalRequest;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Queries.BatchD
{
    public sealed class GetMaintenanceExternalRequestQueryHandlerBatchDTests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetMaintenanceExternalRequestQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMapper.Setup(m => m.Map<List<GetMaintenanceExternalRequestDto>>(It.IsAny<object>()))
                .Returns(new List<GetMaintenanceExternalRequestDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetAllMaintenanceExternalRequestAsync(1, 10, null, It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(((IEnumerable<dynamic>)new List<dynamic>(), 0));

            var result = await CreateSut().Handle(
                new GetMaintenanceExternalRequestQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResults_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllMaintenanceExternalRequestAsync(1, 10, null, It.IsAny<DateTimeOffset?>(), It.IsAny<DateTimeOffset?>()))
                .ReturnsAsync(((IEnumerable<dynamic>)new List<dynamic>(), 0));

            var result = await CreateSut().Handle(
                new GetMaintenanceExternalRequestQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Data.Should().BeEmpty();
        }
    }
}
