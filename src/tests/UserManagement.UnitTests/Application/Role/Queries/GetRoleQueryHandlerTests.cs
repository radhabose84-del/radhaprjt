using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.UserRole.Queries.GetRole;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Role.Queries
{
    public sealed class GetRoleQueryHandlerTests
    {
        private readonly Mock<IUserRoleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockMappingQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IItemGroupLookup> _mockItemGroupLookup = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetRoleQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetRoleQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMappingQueryRepo.Object, _mockItemGroupLookup.Object,
                _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.UserRole> { new() { Id = 1 } };
            var dtoList = new List<GetUserRoleDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllRoleAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetUserRoleDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRoleQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.UserRole> { new() { Id = 1 } };
            var dtoList = new List<GetUserRoleDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllRoleAsync(2, 5, "test"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<GetUserRoleDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRoleQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
