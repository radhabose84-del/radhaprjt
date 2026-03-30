using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Department.Queries
{
    public sealed class GetAllDepartmentQueryHandlerTests
    {
        private readonly Mock<IDepartmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetDepartmentQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetDepartmentQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_WithData_ReturnsSuccess()
        {
            var dtoList = new List<DepartmentDto> { DepartmentBuilders.ValidDto() };
            var mappedList = new List<GetDepartmentDto> { DepartmentBuilders.ValidGetDepartmentDto() };

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
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_WithData_ReturnsPaginationMetadata()
        {
            var dtoList = new List<DepartmentDto> { DepartmentBuilders.ValidDto() };
            var mappedList = new List<GetDepartmentDto> { DepartmentBuilders.ValidGetDepartmentDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentAsync(2, 5, "search"))
                .ReturnsAsync((dtoList, 11));

            _mockMapper
                .Setup(m => m.Map<List<GetDepartmentDto>>(dtoList))
                .Returns(mappedList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepartmentQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsNotSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentAsync(1, 10, null))
                .ReturnsAsync((new List<DepartmentDto>(), 0));

            var result = await CreateSut().Handle(
                new GetDepartmentQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("No Record Found");
        }

        [Fact]
        public async Task Handle_NullResult_ReturnsNotSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentAsync(1, 10, null))
                .ReturnsAsync(((List<DepartmentDto>?)null!, 0));

            var result = await CreateSut().Handle(
                new GetDepartmentQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
