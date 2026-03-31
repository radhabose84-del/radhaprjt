using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.DepartmentGroup.Queries
{
    public sealed class GetDepartmentGroupByIdQueryHandlerTests
    {
        private readonly Mock<IDepartmentGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepartmentGroupByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static UserManagement.Domain.Entities.DepartmentGroup ValidEntity() =>
            new() { Id = 1, DepartmentGroupCode = "DG001", DepartmentGroupName = "Test Group" };

        private static DepartmentGroupByIdDto ValidDto() =>
            new() { Id = 1, DepartmentGroupCode = "DG001", DepartmentGroupName = "Test Group" };

        [Fact]
        public async Task Handle_ExistingId_ReturnsDepartmentGroupDto()
        {
            var entity = ValidEntity();
            var dto = ValidDto();
            var query = new GetDepartmentGroupByIdQuery { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetDepartmentGroupByIdAsync(query.Id))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<DepartmentGroupByIdDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.DepartmentGroupCode.Should().Be("DG001");
        }

        [Fact]
        public async Task Handle_NonExistingId_ThrowsValidationException()
        {
            var query = new GetDepartmentGroupByIdQuery { Id = 99 };

            _mockQueryRepo
                .Setup(r => r.GetDepartmentGroupByIdAsync(query.Id))
                .ReturnsAsync((UserManagement.Domain.Entities.DepartmentGroup?)null);

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var entity = ValidEntity();
            var dto = ValidDto();
            var query = new GetDepartmentGroupByIdQuery { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetDepartmentGroupByIdAsync(query.Id))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<DepartmentGroupByIdDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.Module == "Department"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsRepositoryOnce()
        {
            var entity = ValidEntity();
            var dto = ValidDto();
            var query = new GetDepartmentGroupByIdQuery { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetDepartmentGroupByIdAsync(query.Id))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<DepartmentGroupByIdDto>(entity))
                .Returns(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetDepartmentGroupByIdAsync(query.Id), Times.Once);
        }
    }
}
