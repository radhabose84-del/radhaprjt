using AutoMapper;
using MediatR;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.AccessPolicy.Queries.GetRoleAccessPolicies;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.AccessPolicy.Queries
{
    public sealed class GetRoleAccessPoliciesQueryHandlerTests
    {
        private readonly Mock<IAccessPolicyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper>                      _mockMapper    = new(MockBehavior.Loose);
        private readonly Mock<IMediator>                    _mockMediator  = new(MockBehavior.Loose);

        private GetRoleAccessPoliciesQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsAssignmentList()
        {
            var list = new List<RoleAccessPolicyDto> { AccessPolicyBuilders.ValidRoleAccessPolicyDto() };
            _mockQueryRepo
                .Setup(r => r.GetRoleAccessPoliciesAsync(1, null))
                .ReturnsAsync(list);

            var result = await CreateSut().Handle(
                new GetRoleAccessPoliciesQuery { AccessPolicyId = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].AccessPolicyId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetRoleAccessPoliciesAsync(99, null))
                .ReturnsAsync(new List<RoleAccessPolicyDto>());

            var result = await CreateSut().Handle(
                new GetRoleAccessPoliciesQuery { AccessPolicyId = 99 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithRoleIdFilter_PassesRoleIdToRepo()
        {
            _mockQueryRepo
                .Setup(r => r.GetRoleAccessPoliciesAsync(1, 5))
                .ReturnsAsync(new List<RoleAccessPolicyDto>());

            await CreateSut().Handle(
                new GetRoleAccessPoliciesQuery { AccessPolicyId = 1, RoleId = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetRoleAccessPoliciesAsync(1, 5), Times.Once);
        }

        [Fact]
        public async Task Handle_CallsGetRoleAccessPoliciesOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetRoleAccessPoliciesAsync(1, null))
                .ReturnsAsync(new List<RoleAccessPolicyDto>());

            await CreateSut().Handle(
                new GetRoleAccessPoliciesQuery { AccessPolicyId = 1 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetRoleAccessPoliciesAsync(1, null), Times.Once);
        }
    }
}
