using AutoMapper;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using UserManagement.Application.RoleEntitlements.Commands.GetRolePrivileges;
using UserManagement.Application.RoleEntitlements.Queries.GetRolePrivileges;

namespace UserManagement.UnitTests.Application.RoleEntitlements.Queries
{
    public sealed class GetRolePrivilegesQueryHandlerTests
    {
        private readonly Mock<IRoleEntitlementQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetRolePrivilegesQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ValidUserId_ReturnsModuleList()
        {
            var modules = new List<UserManagement.Domain.Entities.Modules>();
            _mockQueryRepo
                .Setup(r => r.GetRolePrivileges(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(modules);
            _mockMapper
                .Setup(m => m.Map<List<ModuleDTO>>(modules))
                .Returns(new List<ModuleDTO>());

            var result = await CreateSut().Handle(new GetRolePrivilegesQuery { UserId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_ValidUserId_CallsRepositoryOnce()
        {
            var modules = new List<UserManagement.Domain.Entities.Modules>();
            _mockQueryRepo
                .Setup(r => r.GetRolePrivileges(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(modules);
            _mockMapper
                .Setup(m => m.Map<List<ModuleDTO>>(modules))
                .Returns(new List<ModuleDTO>());

            await CreateSut().Handle(new GetRolePrivilegesQuery { UserId = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetRolePrivileges(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidUserId_MapsRepositoryResult()
        {
            var modules = new List<UserManagement.Domain.Entities.Modules> { new() };
            var mappedDtos = new List<ModuleDTO> { new() };
            _mockQueryRepo
                .Setup(r => r.GetRolePrivileges(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(modules);
            _mockMapper
                .Setup(m => m.Map<List<ModuleDTO>>(modules))
                .Returns(mappedDtos);

            var result = await CreateSut().Handle(new GetRolePrivilegesQuery { UserId = 2 }, CancellationToken.None);

            result.Should().BeEquivalentTo(mappedDtos);
        }

        [Fact]
        public async Task Handle_EmptyModules_ReturnsEmptyList()
        {
            var modules = new List<UserManagement.Domain.Entities.Modules>();
            _mockQueryRepo
                .Setup(r => r.GetRolePrivileges(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(modules);
            _mockMapper
                .Setup(m => m.Map<List<ModuleDTO>>(modules))
                .Returns(new List<ModuleDTO>());

            var result = await CreateSut().Handle(new GetRolePrivilegesQuery { UserId = 3 }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
