using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.DepartmentGroup.Queries
{
    public sealed class GetDepartmentGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IDepartmentGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDepartmentGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static List<UserManagement.Domain.Entities.DepartmentGroup> ValidEntityList() =>
            new()
            {
                new() { Id = 1, DepartmentGroupCode = "DG001", DepartmentGroupName = "Group Alpha" },
                new() { Id = 2, DepartmentGroupCode = "DG002", DepartmentGroupName = "Group Beta" }
            };

        private static List<DepartmentGroupAutoCompleteDto> ValidDtoList() =>
            new()
            {
                new() { Id = 1, DepartmentGroupCode = "DG001", DepartmentGroupName = "Group Alpha" },
                new() { Id = 2, DepartmentGroupCode = "DG002", DepartmentGroupName = "Group Beta" }
            };

        [Fact]
        public async Task Handle_MatchingPattern_ReturnsDtoList()
        {
            var entities = ValidEntityList();
            var dtos = ValidDtoList();
            var query = new GetDepartmentGroupAutoCompleteQuery { SearchPattern = "Group" };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentGroupAsync("Group"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<DepartmentGroupAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_MatchingPattern_PublishesAuditEvent()
        {
            var entities = ValidEntityList();
            var dtos = ValidDtoList();
            var query = new GetDepartmentGroupAutoCompleteQuery { SearchPattern = "Group" };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentGroupAsync("Group"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<DepartmentGroupAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAutoComplete" &&
                        e.Module == "DepartmentGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NullPattern_UsesEmptyString()
        {
            var entities = ValidEntityList();
            var dtos = ValidDtoList();
            var query = new GetDepartmentGroupAutoCompleteQuery { SearchPattern = null };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentGroupAsync(string.Empty))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<DepartmentGroupAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NoMatchingPattern_ThrowsValidationException()
        {
            var query = new GetDepartmentGroupAutoCompleteQuery { SearchPattern = "ZZZNOMATCH" };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentGroupAsync("ZZZNOMATCH"))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.DepartmentGroup>());

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No DepartmentGroup found*");
        }
    }
}
