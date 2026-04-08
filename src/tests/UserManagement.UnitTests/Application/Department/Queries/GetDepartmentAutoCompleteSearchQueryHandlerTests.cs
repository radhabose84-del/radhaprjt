using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Department.Queries
{
    public sealed class GetDepartmentAutoCompleteSearchQueryHandlerTests
    {
        private readonly Mock<IDepartmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetDepartmentAutoCompleteSearchQueryHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetDepartmentAutoCompleteSearchQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_SuperAdmin_ReturnsAdminResults()
        {
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("SUPER_ADMIN");

            var entities = new List<UserManagement.Domain.Entities.Department> { new() { Id = 1 } };
            var dtoList = new List<DepartmentAutocompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetDepartment_SuperAdmin(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<DepartmentAutocompleteDto>>(entities))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetDepartmentAutoCompleteSearchQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_RegularUser_CallsAutoCompleteSearch()
        {
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("USER");

            var entities = new List<UserManagement.Domain.Entities.Department> { new() { Id = 1 } };
            var dtoList = new List<DepartmentAutocompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentAutoCompleteSearchAsync(It.IsAny<string?>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<DepartmentAutocompleteDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepartmentAutoCompleteSearchQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
