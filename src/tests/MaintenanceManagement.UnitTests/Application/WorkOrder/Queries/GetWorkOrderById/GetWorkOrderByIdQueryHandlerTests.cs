using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IWorkOrder;
using MaintenanceManagement.Application.WorkOrder.Queries.GetWorkOrderById;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkOrder.Queries.GetWorkOrderByIdTests
{
    public sealed class GetWorkOrderByIdQueryHandlerTests
    {
        private readonly Mock<IWorkOrderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IWorkOrderCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetWorkOrderByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockIp.Object, _mockCommandRepo.Object, _mockDeptLookup.Object);

        [Fact]
        public async Task Handle_NullWorkOrder_ReturnsNotFound()
        {
            _mockQueryRepo.Setup(r => r.GetWorkOrderByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((((dynamic)null!),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>()));

            var result = await CreateSut().Handle(new GetWorkOrderByIdQuery { Id = 99 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("not found");
        }

        [Fact]
        public async Task Handle_DepartmentNotInLookup_ReturnsNotFoundMessage()
        {
            dynamic woResult = new System.Dynamic.ExpandoObject();
            woResult.Id = 1;

            _mockQueryRepo.Setup(r => r.GetWorkOrderByIdAsync(1))
                .ReturnsAsync(((dynamic)woResult,
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>()));

            _mockMapper.Setup(m => m.Map<GetWorkOrderByIdDto>(It.IsAny<object>()))
                .Returns(new GetWorkOrderByIdDto { DepartmentId = 99 });

            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>
                {
                    new() { DepartmentId = 1, DepartmentName = "Dept1" }
                });

            var result = await CreateSut().Handle(new GetWorkOrderByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidQuery_CallsRepositoryOnce()
        {
            _mockQueryRepo.Setup(r => r.GetWorkOrderByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((((dynamic)null!),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>()));

            await CreateSut().Handle(new GetWorkOrderByIdQuery { Id = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetWorkOrderByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task Handle_DepartmentMatches_ReturnsSuccessWithData()
        {
            dynamic woResult = new System.Dynamic.ExpandoObject();
            woResult.Id = 1;

            _mockQueryRepo.Setup(r => r.GetWorkOrderByIdAsync(1))
                .ReturnsAsync(((dynamic)woResult,
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>(),
                    (IEnumerable<dynamic>)new List<dynamic>()));

            _mockMapper.Setup(m => m.Map<GetWorkOrderByIdDto>(It.IsAny<object>()))
                .Returns(new GetWorkOrderByIdDto { DepartmentId = 1 });

            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>
                {
                    new() { DepartmentId = 1, DepartmentName = "Dept1" }
                });

            var result = await CreateSut().Handle(new GetWorkOrderByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }
    }
}
