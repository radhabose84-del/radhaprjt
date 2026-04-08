using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Application.UserRole.Queries.GetRolesAutocomplete;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Role.Queries
{
    public sealed class GetRolesAutocompleteQueryHandlerTests
    {
        private readonly Mock<IUserRoleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetRolesAutocompleteQueryHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetRolesAutocompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_SuperAdmin_ReturnsAdminResults()
        {
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("SUPER_ADMIN");

            var entities = new List<UserManagement.Domain.Entities.UserRole> { new() { Id = 1 } };
            var dtoList = new List<GetUserRoleAutocompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetRoles_SuperAdmin(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetUserRoleAutocompleteDto>>(entities))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetRolesAutocompleteQuery { SearchTerm = "admin" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_RegularUser_CallsStandardSearch()
        {
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("USER");

            var entities = new List<UserManagement.Domain.Entities.UserRole> { new() { Id = 1 } };
            var dtoList = new List<GetUserRoleAutocompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetRolesAsync(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<GetUserRoleAutocompleteDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRolesAutocompleteQuery { SearchTerm = "test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
