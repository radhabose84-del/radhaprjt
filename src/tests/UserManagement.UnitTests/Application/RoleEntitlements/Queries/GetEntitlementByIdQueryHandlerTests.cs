using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Queries.GetRoleEntitlementById;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleEntitlements.Queries
{
    public sealed class GetEntitlementByIdQueryHandlerTests
    {
        private readonly Mock<IRoleEntitlementCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IRoleEntitlementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetEntitlementByIdQueryHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsMappedDto()
        {
            var roleId = new UserManagement.Domain.Entities.UserRole { Id = 1 };
            IList<RoleModule> roleModules = new List<RoleModule>();
            IList<RoleParent> parentMenus = new List<RoleParent>();
            IList<RoleChild> childMenus = new List<RoleChild>();
            IList<RoleMenuPrivileges> roleMenuPrivileges = new List<RoleMenuPrivileges>();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((roleId, roleModules, parentMenus, childMenus, roleMenuPrivileges));

            var expectedDto = new GetByIdRoleEntitlementDTO { RoleId = 1 };

            _mockMapper
                .Setup(m => m.Map<GetByIdRoleEntitlementDTO>(It.IsAny<object>()))
                .Returns(expectedDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRoleEntitlementByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.RoleId.Should().Be(1);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var roleId = new UserManagement.Domain.Entities.UserRole { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((roleId, (IList<RoleModule>)new List<RoleModule>(), (IList<RoleParent>)new List<RoleParent>(), (IList<RoleChild>)new List<RoleChild>(), (IList<RoleMenuPrivileges>)new List<RoleMenuPrivileges>()));

            _mockMapper
                .Setup(m => m.Map<GetByIdRoleEntitlementDTO>(It.IsAny<object>()))
                .Returns(new GetByIdRoleEntitlementDTO());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetRoleEntitlementByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
