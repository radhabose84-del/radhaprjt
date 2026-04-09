using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Application.Entity.Queries.GetEntityAutoComplete;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Entity.Queries
{
    public sealed class GetEntityAutocompleteQueryHandlerTests
    {
        private readonly Mock<IEntityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetEntityAutocompleteQueryHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetEntityAutocompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_SuperAdmin_ReturnsAdminResults()
        {
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("SUPER_ADMIN");

            var entities = new List<UserManagement.Domain.Entities.Entity> { new() { Id = 1 } };
            var dtoList = new List<EntityAutoCompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetByEntityName_SuperAdmin(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<EntityAutoCompleteDto>>(entities))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetEntityAutocompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_RegularUser_CallsEntityAutoComplete()
        {
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("USER");
            _mockIpService.Setup(s => s.GetUserId()).Returns(42);

            var entities = new List<UserManagement.Domain.Entities.Entity> { new() { Id = 1 } };
            var dtoList = new List<EntityAutoCompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetByEntityNameAsync(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<EntityAutoCompleteDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetEntityAutocompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
