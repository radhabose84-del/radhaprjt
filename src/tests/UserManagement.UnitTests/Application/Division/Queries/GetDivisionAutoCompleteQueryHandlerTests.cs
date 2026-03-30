using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Divisions.Queries.GetDivisionAutoComplete;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Division.Queries
{
    public sealed class GetDivisionAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDivisionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Strict);

        private GetDivisionAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_AdminUser_ReturnsSuperAdminList()
        {
            var entities = new List<UserManagement.Domain.Entities.Division> { DivisionBuilders.ValidEntity() };
            var dtos = DivisionBuilders.ValidAutoCompleteList();

            _mockIpService
                .Setup(s => s.GetGroupCode())
                .Returns("SUPER_ADMIN");

            _mockQueryRepo
                .Setup(r => r.GetDivision_SuperAdmin(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<DivisionAutoCompleteDTO>>(entities))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetDivisionAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Test Division");
        }

        [Fact]
        public async Task Handle_RegularUser_ReturnsFilteredList()
        {
            var entities = new List<UserManagement.Domain.Entities.Division> { DivisionBuilders.ValidEntity() };
            var dtos = DivisionBuilders.ValidAutoCompleteList();

            _mockIpService
                .Setup(s => s.GetGroupCode())
                .Returns("USER");

            _mockQueryRepo
                .Setup(r => r.GetDivision(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<DivisionAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDivisionAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Test Division");
        }

        [Fact]
        public async Task Handle_RegularUser_PublishesAuditEvent()
        {
            var entities = new List<UserManagement.Domain.Entities.Division> { DivisionBuilders.ValidEntity() };
            var dtos = DivisionBuilders.ValidAutoCompleteList();

            _mockIpService
                .Setup(s => s.GetGroupCode())
                .Returns("USER");

            _mockQueryRepo
                .Setup(r => r.GetDivision(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<DivisionAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetDivisionAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.Module == "Division"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
