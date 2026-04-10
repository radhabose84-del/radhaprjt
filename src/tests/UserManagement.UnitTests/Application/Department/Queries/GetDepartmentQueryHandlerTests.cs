using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Department.Queries
{
    public sealed class GetDepartmentQueryHandlerTests
    {
        private readonly Mock<IDepartmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetDepartmentQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetDepartmentQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<DepartmentDto> { new() { Id = 1 } };
            var mappedList = new List<GetDepartmentDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetDepartmentDto>>(dtoList))
                .Returns(mappedList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepartmentQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<DepartmentDto> { new() { Id = 1 } };
            var mappedList = new List<GetDepartmentDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentAsync(2, 5, "test"))
                .ReturnsAsync((dtoList, 11));

            _mockMapper
                .Setup(m => m.Map<List<GetDepartmentDto>>(dtoList))
                .Returns(mappedList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepartmentQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
