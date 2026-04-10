using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Departments.Queries.GetDepartmentByGroupWithControl;

namespace UserManagement.UnitTests.Application.Department.Queries
{
    public sealed class GetDepartmentByGroupWithControlQueryHandlerTests
    {
        private readonly Mock<IDepartmentQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepartmentByGroupWithControlQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_NoData_ReturnsFailure()
        {
            _mockRepo.Setup(r => r.GetDepartmentsByDepartmentGroupWithControl("NonExistent"))
                .ReturnsAsync((List<DepartmentWithControlByGroupDto>?)null);

            var result = await CreateSut().Handle(new GetDepartmentByGroupWithControlQuery { DepartmentGroupName = "NonExistent" }, CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }
    }
}
