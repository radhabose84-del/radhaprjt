using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Application.RoleItemGroupMapping.Queries.GetAllRoleItemGroupMapping;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleItemGroupMapping.Queries
{
    public sealed class GetAllRoleItemGroupMappingQueryHandlerTests
    {
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IItemGroupLookup> _mockItemGroupLookup = new(MockBehavior.Loose);

        private GetAllRoleItemGroupMappingQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockItemGroupLookup.Object);

        private void SetupHappyPath(
            List<UserManagement.Domain.Entities.RoleItemGroupMapping> entities,
            List<RoleItemGroupMappingDto> dtos,
            int totalCount = 1)
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((entities, totalCount));

            _mockMapper
                .Setup(m => m.Map<List<RoleItemGroupMappingDto>>(
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
        public async Task Handle_ReturnsSuccess()
        {
            SetupHappyPath(
                new List<UserManagement.Domain.Entities.RoleItemGroupMapping>(),
                new List<RoleItemGroupMappingDto>(), 0);

            var result = await CreateSut().Handle(
                new GetAllRoleItemGroupMappingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsData()
        {
            var entities = new List<UserManagement.Domain.Entities.RoleItemGroupMapping>
            {
                new() { Id = 1, RoleId = 1, ItemGroupId = 2 }
            };
            var dtos = new List<RoleItemGroupMappingDto>
            {
                new() { Id = 1, RoleId = 1, ItemGroupId = 2 }
            };
            SetupHappyPath(entities, dtos, 1);

            var result = await CreateSut().Handle(
                new GetAllRoleItemGroupMappingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            SetupHappyPath(
                new List<UserManagement.Domain.Entities.RoleItemGroupMapping>(),
                new List<RoleItemGroupMappingDto>(), 5);

            var result = await CreateSut().Handle(
                new GetAllRoleItemGroupMappingQuery { PageNumber = 2, PageSize = 5 },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(5);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath(
                new List<UserManagement.Domain.Entities.RoleItemGroupMapping>(),
                new List<RoleItemGroupMappingDto>(), 0);

            await CreateSut().Handle(
                new GetAllRoleItemGroupMappingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            SetupHappyPath(
                new List<UserManagement.Domain.Entities.RoleItemGroupMapping>(),
                new List<RoleItemGroupMappingDto>(), 0);

            var result = await CreateSut().Handle(
                new GetAllRoleItemGroupMappingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PopulatesItemGroupName_WhenLookupMatches()
        {
            var entities = new List<UserManagement.Domain.Entities.RoleItemGroupMapping>
            {
                new() { Id = 1, RoleId = 1, ItemGroupId = 10 }
            };
            var dtos = new List<RoleItemGroupMappingDto>
            {
                new() { Id = 1, RoleId = 1, ItemGroupId = 10 }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((entities, 1));
            _mockMapper
                .Setup(m => m.Map<List<RoleItemGroupMappingDto>>(
                    It.IsAny<List<UserManagement.Domain.Entities.RoleItemGroupMapping>>()))
                .Returns(dtos);
            _mockItemGroupLookup
                .Setup(l => l.GetAllItemGroupsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemGroupLookupDto> { new() { Id = 10, ItemGroupName = "Electronics" } });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllRoleItemGroupMappingQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data[0].ItemGroupName.Should().Be("Electronics");
        }
    }
}
