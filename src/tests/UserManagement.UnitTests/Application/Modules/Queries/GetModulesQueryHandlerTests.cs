using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IModule;
using UserManagement.Application.Modules.Queries.GetModules;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Modules.Queries
{
    public sealed class GetModulesQueryHandlerTests
    {
        private readonly Mock<IModuleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetModulesQueryHandler>> _mockLogger = new();

        private GetModulesQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Modules>
            {
                new() { Id = 1, ModuleName = "Inventory", IsDeleted = false }
            };
            var dtos = new List<ModuleDto> { new ModuleDto { ModuleName = "Inventory" } };

            _mockQueryRepo
                .Setup(r => r.GetAllModulesAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<ModuleDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetModulesQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Modules>
            {
                new() { Id = 2, ModuleName = "Sales", IsDeleted = false }
            };
            var dtos = new List<ModuleDto> { new ModuleDto { ModuleName = "Sales" } };

            _mockQueryRepo
                .Setup(r => r.GetAllModulesAsync(2, 5, "Sales"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<ModuleDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetModulesQuery { PageNumber = 2, PageSize = 5, SearchTerm = "Sales" },
                CancellationToken.None);

            // Assert
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.Modules>();
            var dtos = new List<ModuleDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllModulesAsync(1, 10, null))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<ModuleDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetModulesQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
