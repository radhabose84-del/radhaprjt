using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces;
using UserManagement.Application.Departments.Queries.GetDepartmentAutoCompleteSearch;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.Department.Queries
{
    public sealed class GetDepartmentAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDepartmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetDepartmentAutoCompleteSearchQueryHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpAddressService = new(MockBehavior.Strict);

        private GetDepartmentAutoCompleteSearchQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object, _mockIpAddressService.Object);

        [Fact]
        public async Task Handle_NonAdmin_WithResults_ReturnsDtos()
        {
            var entityList = new List<UserManagement.Domain.Entities.Department>
            {
                DepartmentBuilders.ValidEntity()
            };
            var dtoList = DepartmentBuilders.ValidAutoCompleteList();

            _mockIpAddressService
                .Setup(s => s.GetGroupCode())
                .Returns("USER");

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentAutoCompleteSearchAsync("Test"))
                .ReturnsAsync(entityList);

            _mockMapper
                .Setup(m => m.Map<List<DepartmentAutocompleteDto>>(entityList))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepartmentAutoCompleteSearchQuery { SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].DeptName.Should().Be("Test Department");
        }

        [Fact]
        public async Task Handle_SuperAdmin_ReturnsSuperAdminResults()
        {
            var entityList = new List<UserManagement.Domain.Entities.Department>
            {
                DepartmentBuilders.ValidEntity()
            };
            var dtoList = DepartmentBuilders.ValidAutoCompleteList();

            _mockIpAddressService
                .Setup(s => s.GetGroupCode())
                .Returns("SUPER_ADMIN");

            _mockQueryRepo
                .Setup(r => r.GetDepartment_SuperAdmin("Test"))
                .ReturnsAsync(entityList);

            _mockMapper
                .Setup(m => m.Map<List<DepartmentAutocompleteDto>>(entityList))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetDepartmentAutoCompleteSearchQuery { SearchPattern = "Test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NonAdmin_NoResults_ThrowsValidationException()
        {
            _mockIpAddressService
                .Setup(s => s.GetGroupCode())
                .Returns("USER");

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentAutoCompleteSearchAsync("NoMatch"))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.Department>());

            Func<Task> act = async () => await CreateSut().Handle(
                new GetDepartmentAutoCompleteSearchQuery { SearchPattern = "NoMatch" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No Record Found*");
        }
    }
}
