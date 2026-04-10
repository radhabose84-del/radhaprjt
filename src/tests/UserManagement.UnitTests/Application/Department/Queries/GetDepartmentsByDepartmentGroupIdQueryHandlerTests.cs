using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Departments.Queries.GetDepartmentByDepartmentGroupId;
using UserManagement.Application.Departments.Queries.GetDepartments;

namespace UserManagement.UnitTests.Application.Department.Queries
{
    public sealed class GetDepartmentsByDepartmentGroupIdQueryHandlerTests
    {
        private readonly Mock<IDepartmentQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepartmentsByDepartmentGroupIdQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyGroupName_ReturnsFailure()
        {
            var result = await CreateSut().Handle(new GetDepartmentsByDepartmentGroupIdQuery { DepartmentGroupName = "" }, CancellationToken.None);
            result.IsSuccess.Should().BeFalse();
        }
    }
}
