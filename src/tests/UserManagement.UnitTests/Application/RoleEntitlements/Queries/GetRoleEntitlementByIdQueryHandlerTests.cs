using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlementById;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleEntitlements.Queries
{
    public sealed class GetRoleEntitlementByIdQueryHandlerTests
    {
        private readonly Mock<IRoleEntitlementCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRoleEntitlementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetEntitlementByIdQueryHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupGetByIdAsync(int id, UserRole roleResult)
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((
                    roleResult,
                    (IList<RoleModule>)new List<RoleModule>(),
                    (IList<RoleParent>)new List<RoleParent>(),
                    (IList<RoleChild>)new List<RoleChild>(),
                    (IList<RoleMenuPrivileges>)new List<RoleMenuPrivileges>()
                ));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockMapper
                .Setup(m => m.Map<GetByIdRoleEntitlementDTO>(It.IsAny<object>()))
                .Returns(new GetByIdRoleEntitlementDTO());
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsNonNullDto()
        {
            var role = new UserRole { Id = 1, RoleName = "Admin" };
            SetupGetByIdAsync(1, role);

            var result = await CreateSut().Handle(new GetRoleEntitlementByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidId_CallsGetByIdOnce()
        {
            var role = new UserRole { Id = 2, RoleName = "Manager" };
            SetupGetByIdAsync(2, role);

            await CreateSut().Handle(new GetRoleEntitlementByIdQuery { Id = 2 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(2), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var role = new UserRole { Id = 3, RoleName = "User" };
            SetupGetByIdAsync(3, role);

            await CreateSut().Handle(new GetRoleEntitlementByIdQuery { Id = 3 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "RoleEntitlement"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_CallsMapperWithTupleResult()
        {
            var role = new UserRole { Id = 4, RoleName = "Viewer" };
            SetupGetByIdAsync(4, role);

            await CreateSut().Handle(new GetRoleEntitlementByIdQuery { Id = 4 }, CancellationToken.None);

            _mockMapper.Verify(m => m.Map<GetByIdRoleEntitlementDTO>(It.IsAny<object>()), Times.Once);
        }
    }
}
