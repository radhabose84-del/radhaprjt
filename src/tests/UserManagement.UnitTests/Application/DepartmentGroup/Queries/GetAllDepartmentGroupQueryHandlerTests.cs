using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Queries.GetAllDepartmentGroup;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.DepartmentGroup.Queries
{
    public sealed class GetAllDepartmentGroupQueryHandlerTests
    {
        private readonly Mock<IDepartmentGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllDepartmentGroupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static List<UserManagement.Domain.Entities.DepartmentGroup> ValidEntityList() =>
            new()
            {
                new() { Id = 1, DepartmentGroupCode = "DG001", DepartmentGroupName = "Group One" },
                new() { Id = 2, DepartmentGroupCode = "DG002", DepartmentGroupName = "Group Two" }
            };

        private static List<GetAllDepartmentGroupDto> ValidDtoList() =>
            new()
            {
                new() { Id = 1, DepartmentGroupCode = "DG001", DepartmentGroupName = "Group One" },
                new() { Id = 2, DepartmentGroupCode = "DG002", DepartmentGroupName = "Group Two" }
            };

        [Fact]
        public async Task Handle_WithData_ReturnsSuccess()
        {
            var entities = ValidEntityList();
            var dtos = ValidDtoList();
            var query = new GetAllDepartmentGroupQuery { PageNumber = 1, PageSize = 10 };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentGroupAsync(query.PageNumber, query.PageSize, query.SearchTerm))
                .ReturnsAsync((entities, 2));

            _mockMapper
                .Setup(m => m.Map<List<GetAllDepartmentGroupDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailure()
        {
            var query = new GetAllDepartmentGroupQuery { PageNumber = 1, PageSize = 10 };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentGroupAsync(query.PageNumber, query.PageSize, query.SearchTerm))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.DepartmentGroup>(), 0));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No Record Found");
        }

        [Fact]
        public async Task Handle_WithData_PublishesAuditEvent()
        {
            var entities = ValidEntityList();
            var dtos = ValidDtoList();
            var query = new GetAllDepartmentGroupQuery { PageNumber = 1, PageSize = 10 };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentGroupAsync(query.PageNumber, query.PageSize, query.SearchTerm))
                .ReturnsAsync((entities, 2));

            _mockMapper
                .Setup(m => m.Map<List<GetAllDepartmentGroupDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(query, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.Module == "DepartmentGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithData_ReturnsPaginationMetadata()
        {
            var entities = ValidEntityList();
            var dtos = ValidDtoList();
            var query = new GetAllDepartmentGroupQuery { PageNumber = 2, PageSize = 5 };

            _mockQueryRepo
                .Setup(r => r.GetAllDepartmentGroupAsync(query.PageNumber, query.PageSize, query.SearchTerm))
                .ReturnsAsync((entities, 12));

            _mockMapper
                .Setup(m => m.Map<List<GetAllDepartmentGroupDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(12);
        }
    }
}
