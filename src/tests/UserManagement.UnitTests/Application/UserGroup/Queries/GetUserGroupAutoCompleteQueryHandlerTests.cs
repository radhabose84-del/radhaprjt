using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Queries.GetUserGroupAutoComplete;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.UserGroup.Queries
{
    public sealed class GetUserGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IUserGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUserGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_MatchingResults_ReturnsDtoList()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.UserGroup>
            {
                new() { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" }
            };
            var dtos = new List<UserGroupAutoCompleteDto>
            {
                new() { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" }
            };

            _mockQueryRepo
                .Setup(r => r.GetUserGroups("Test"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UserGroupAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetUserGroupAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].GroupCode.Should().Be("GRP001");
        }

        [Fact]
        public async Task Handle_NoMatchingResults_ThrowsValidationException()
        {
            // Arrange
            _mockQueryRepo
                .Setup(r => r.GetUserGroups("xyz"))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.UserGroup>());

            var sut = CreateSut();

            // Act
            Func<Task> act = async () =>
                await sut.Handle(
                    new GetUserGroupAutoCompleteQuery { SearchPattern = "xyz" },
                    CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No user group found*");
        }

        [Fact]
        public async Task Handle_NullPattern_UsesEmptyString()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.UserGroup>
            {
                new() { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" }
            };
            var dtos = new List<UserGroupAutoCompleteDto> { new() { Id = 1, GroupCode = "GRP001" } };

            _mockQueryRepo
                .Setup(r => r.GetUserGroups(string.Empty))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UserGroupAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(
                new GetUserGroupAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_MatchingResults_PublishesAuditEvent()
        {
            // Arrange
            var entities = new List<UserManagement.Domain.Entities.UserGroup>
            {
                new() { Id = 1, GroupCode = "GRP001", GroupName = "Test Group" }
            };
            var dtos = new List<UserGroupAutoCompleteDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetUserGroups("Test"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UserGroupAutoCompleteDto>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(
                new GetUserGroupAutoCompleteQuery { SearchPattern = "Test" },
                CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetAutoComplete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
