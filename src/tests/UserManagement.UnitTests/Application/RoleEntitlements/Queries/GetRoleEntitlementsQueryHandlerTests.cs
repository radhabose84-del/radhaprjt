using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlements;

namespace UserManagement.UnitTests.Application.RoleEntitlements.Queries
{
    public sealed class GetRoleEntitlementsQueryHandlerTests
    {
        private readonly Mock<IRoleEntitlementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRoleEntitlementsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_AnyQuery_ReturnsEmptyList()
        {
            var query = new GetRoleEntitlementsQuery { RoleName = "Admin" };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_AnyQuery_ReturnsListType()
        {
            var query = new GetRoleEntitlementsQuery { RoleName = "User" };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().BeOfType<List<RoleEntitlementDto>>();
        }

        [Fact]
        public async Task Handle_DoesNotCallRepository()
        {
            var query = new GetRoleEntitlementsQuery { RoleName = "Admin" };

            await CreateSut().Handle(query, CancellationToken.None);

            // Handler body is stubbed — no repo calls expected
            _mockQueryRepo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_MultipleInvocations_EachReturnsEmptyList()
        {
            var sut = CreateSut();

            var result1 = await sut.Handle(new GetRoleEntitlementsQuery { RoleName = "Admin" }, CancellationToken.None);
            var result2 = await sut.Handle(new GetRoleEntitlementsQuery { RoleName = "User" }, CancellationToken.None);

            result1.Should().BeEmpty();
            result2.Should().BeEmpty();
        }
    }
}
