using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Dto;
using UserManagement.Application.RoleItemGroupMapping.Queries.GetRoleItemGroupMappingById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.RoleItemGroupMapping.Queries
{
    public sealed class GetRoleItemGroupMappingByIdQueryHandlerTests
    {
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IItemGroupLookup> _mockItemGroupLookup = new(MockBehavior.Loose);

        private GetRoleItemGroupMappingByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockItemGroupLookup.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var entity = new UserManagement.Domain.Entities.RoleItemGroupMapping { Id = 1, RoleId = 1, ItemGroupId = 2 };
            var dto = new RoleItemGroupMappingDto { Id = 1, RoleId = 1, ItemGroupId = 2 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<RoleItemGroupMappingDto>(entity)).Returns(dto);
            _mockItemGroupLookup
                .Setup(l => l.GetAllItemGroupsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemGroupLookupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRoleItemGroupMappingByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((UserManagement.Domain.Entities.RoleItemGroupMapping?)null);

            var sut = CreateSut();
            Func<Task> act = async () =>
                await sut.Handle(new GetRoleItemGroupMappingByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entity = new UserManagement.Domain.Entities.RoleItemGroupMapping { Id = 5, RoleId = 1, ItemGroupId = 2 };
            var dto = new RoleItemGroupMappingDto { Id = 5, RoleId = 1, ItemGroupId = 2 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<RoleItemGroupMappingDto>(entity)).Returns(dto);
            _mockItemGroupLookup
                .Setup(l => l.GetAllItemGroupsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemGroupLookupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetRoleItemGroupMappingByIdQuery { Id = 5 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PopulatesItemGroupName_WhenLookupMatches()
        {
            var entity = new UserManagement.Domain.Entities.RoleItemGroupMapping { Id = 1, RoleId = 1, ItemGroupId = 10 };
            var dto = new RoleItemGroupMappingDto { Id = 1, RoleId = 1, ItemGroupId = 10 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<RoleItemGroupMappingDto>(entity)).Returns(dto);
            _mockItemGroupLookup
                .Setup(l => l.GetAllItemGroupsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemGroupLookupDto> { new() { Id = 10, ItemGroupName = "Electronics" } });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRoleItemGroupMappingByIdQuery { Id = 1 }, CancellationToken.None);

            result.ItemGroupName.Should().Be("Electronics");
        }

        [Fact]
        public async Task Handle_NotFound_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((UserManagement.Domain.Entities.RoleItemGroupMapping?)null);

            var sut = CreateSut();
            try { await sut.Handle(new GetRoleItemGroupMappingByIdQuery { Id = 99 }, CancellationToken.None); }
            catch { /* expected */ }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
