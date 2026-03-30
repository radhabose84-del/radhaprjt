using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingByRoleId;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleItemGroupMapping.Queries
{
    public sealed class GetRoleItemGroupMappingByRoleIdQueryHandlerTests
    {
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IItemGroupLookup> _mockItemGroupLookup = new(MockBehavior.Loose);

        private GetRoleItemGroupMappingByRoleIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockItemGroupLookup.Object);

        private void SetupHappyPath(
            List<UserManagement.Domain.Entities.RoleItemGroupMapping> entities,
            List<RoleItemGroupMappingLookupDto> dtos)
        {
            _mockQueryRepo
                .Setup(r => r.GetByRoleIdAsync(It.IsAny<int>()))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<RoleItemGroupMappingLookupDto>>(
                    It.IsAny<List<UserManagement.Domain.Entities.RoleItemGroupMapping>>()))
                .Returns(dtos);
            _mockItemGroupLookup
                .Setup(l => l.GetAllItemGroupsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemGroupLookupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidRoleId_ReturnsList()
        {
            var entities = new List<UserManagement.Domain.Entities.RoleItemGroupMapping>
            {
                new() { Id = 1, RoleId = 1, ItemGroupId = 2 }
            };
            var dtos = new List<RoleItemGroupMappingLookupDto>
            {
                new() { Id = 1, RoleId = 1, ItemGroupId = 2 }
            };
            SetupHappyPath(entities, dtos);

            var result = await CreateSut().Handle(
                new GetRoleItemGroupMappingByRoleIdQuery { RoleId = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupHappyPath(
                new List<UserManagement.Domain.Entities.RoleItemGroupMapping>(),
                new List<RoleItemGroupMappingLookupDto>());

            var result = await CreateSut().Handle(
                new GetRoleItemGroupMappingByRoleIdQuery { RoleId = 99 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath(
                new List<UserManagement.Domain.Entities.RoleItemGroupMapping>(),
                new List<RoleItemGroupMappingLookupDto>());

            await CreateSut().Handle(
                new GetRoleItemGroupMappingByRoleIdQuery { RoleId = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PopulatesItemGroupName_WhenLookupMatches()
        {
            var entities = new List<UserManagement.Domain.Entities.RoleItemGroupMapping>
            {
                new() { Id = 1, RoleId = 1, ItemGroupId = 10 }
            };
            var dtos = new List<RoleItemGroupMappingLookupDto>
            {
                new() { Id = 1, RoleId = 1, ItemGroupId = 10 }
            };

            _mockQueryRepo
                .Setup(r => r.GetByRoleIdAsync(1))
                .ReturnsAsync(entities);
            _mockMapper
                .Setup(m => m.Map<List<RoleItemGroupMappingLookupDto>>(
                    It.IsAny<List<UserManagement.Domain.Entities.RoleItemGroupMapping>>()))
                .Returns(dtos);
            _mockItemGroupLookup
                .Setup(l => l.GetAllItemGroupsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemGroupLookupDto> { new() { Id = 10, ItemGroupName = "Spare Parts" } });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRoleItemGroupMappingByRoleIdQuery { RoleId = 1 }, CancellationToken.None);

            result[0].ItemGroupName.Should().Be("Spare Parts");
        }

        [Fact]
        public async Task Handle_CallsGetByRoleIdOnce()
        {
            SetupHappyPath(
                new List<UserManagement.Domain.Entities.RoleItemGroupMapping>(),
                new List<RoleItemGroupMappingLookupDto>());

            await CreateSut().Handle(
                new GetRoleItemGroupMappingByRoleIdQuery { RoleId = 5 }, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByRoleIdAsync(5), Times.Once);
        }
    }
}
