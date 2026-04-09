using AutoMapper;
using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Application.UserRole.Queries.GetRoleById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Role.Queries
{
    public sealed class GetRoleByIdQueryHandlerTests
    {
        private readonly Mock<IUserRoleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockMappingQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemGroupLookup> _mockItemGroupLookup = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetRoleByIdQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetRoleByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMappingQueryRepo.Object, _mockItemGroupLookup.Object,
                _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ExistingRole_ReturnsMappedDto()
        {
            var entity = new UserManagement.Domain.Entities.UserRole { Id = 1, RoleName = "Admin" };
            var dto = new GetUserRoleDto { Id = 1, RoleName = "Admin" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetUserRoleDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRoleByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.UserRole?)null);

            Func<Task> act = () => CreateSut().Handle(
                new GetRoleByIdQuery { Id = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
