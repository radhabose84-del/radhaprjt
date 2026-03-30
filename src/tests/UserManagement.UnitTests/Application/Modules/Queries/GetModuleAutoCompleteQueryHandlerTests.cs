using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Modules.Queries.GetModuleAutoComplete;
using UserManagement.Application.Modules.Queries.GetModules;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Modules.Queries
{
    public sealed class GetModuleAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IModuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetModuleAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_MatchingPattern_ReturnsDtoList()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Modules>
            {
                new() { Id = 1, ModuleName = "Inventory" }
            };
            var dtos = new List<ModuleAutoCompleteDTO>
            {
                new() { Id = 1, ModuleName = "Inventory" }
            };

            _mockQueryRepo
                .Setup(r => r.GetModule("Inv"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<ModuleAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetModuleAutoCompleteQuery { SearchPattern = "Inv" },
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].ModuleName.Should().Be("Inventory");
        }

        [Fact]
        public async Task Handle_NoMatch_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Modules>();
            var dtos = new List<ModuleAutoCompleteDTO>();

            _mockQueryRepo
                .Setup(r => r.GetModule("XYZ"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<ModuleAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetModuleAutoCompleteQuery { SearchPattern = "XYZ" },
                CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Modules>
            {
                new() { Id = 3, ModuleName = "HR" }
            };
            var dtos = new List<ModuleAutoCompleteDTO>
            {
                new() { Id = 3, ModuleName = "HR" }
            };

            _mockQueryRepo
                .Setup(r => r.GetModule("HR"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<ModuleAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetModuleAutoCompleteQuery { SearchPattern = "HR" },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetModule"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
