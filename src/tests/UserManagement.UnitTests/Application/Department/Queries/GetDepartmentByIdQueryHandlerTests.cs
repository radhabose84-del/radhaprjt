using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Departments.Queries.GetDepartmentById;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Application.Common.Interfaces.IDepartment;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using FluentValidation;

namespace UserManagement.UnitTests.Application.Department.Queries
{
    public sealed class GetDepartmentByIdQueryHandlerTests
    {
        private readonly Mock<IDepartmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetDepartmentByIdQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetDepartmentByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsGetDepartmentDto()
        {
            var entity = DepartmentBuilders.ValidEntity();
            var dto = DepartmentBuilders.ValidGetDepartmentDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetDepartmentDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDepartmentByIdQuery { DepartmentId = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.DeptName.Should().Be("Test Department");
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            var entity = DepartmentBuilders.ValidEntity();
            var dto = DepartmentBuilders.ValidGetDepartmentDto();

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetDepartmentDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetDepartmentByIdQuery { DepartmentId = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "Department"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.Department?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetDepartmentByIdQuery { DepartmentId = 999 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }
    }
}
