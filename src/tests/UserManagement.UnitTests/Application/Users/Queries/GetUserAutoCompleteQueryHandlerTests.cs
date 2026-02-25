using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Users.Queries.GetUserAutoComplete;
using UserManagement.Domain.Entities;                   // for User
using UserManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.UnitTests.Application.Users.Queries
{
    public class GetUserAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IUserQueryRepository> _repo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetUserAutoCompleteQueryHandler>> _logger = new();

        private GetUserAutoCompleteQueryHandler CreateSut()
            => new GetUserAutoCompleteQueryHandler(_repo.Object, _mapper.Object, _mediator.Object, _logger.Object);

        [Fact]
        public async Task Handle_WhenMatchesExist_ReturnsMappedDtos_AndPublishesAuditEvent()
        {
            // Arrange
            var q = new GetUserAutoCompleteQuery { SearchPattern = "ne" };

            var users = new List<User>
            {
                new User { UserId = 1, UserName = "neo", FirstName = "Neo", LastName = "Anderson" },
                new User { UserId = 2, UserName = "nemo", FirstName = "Ne", LastName = "Mo" }
            };

            var mapped = new List<UserAutoCompleteDto>
            {
                new UserAutoCompleteDto { UserId = 1, UserName = "neo" },
                new UserAutoCompleteDto { UserId = 2, UserName = "nemo" }
            };

            _repo.Setup(r => r.GetUser("ne")).ReturnsAsync(users);

            _mediator
                .Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.Module == "User" &&
                        e.ActionDetail == "GetAutoComplete" &&
                        e.ActionName == "ne" &&
                        e.Details!.Contains("was searched")),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(mp => mp.Map<List<UserAutoCompleteDto>>(users))
                .Returns(mapped);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<List<UserAutoCompleteDto>> result =
                await sut.Handle(q, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Count.Should().Be(2);
            result.Data[0].UserName.Should().Be("neo");

            _repo.Verify(r => r.GetUser("ne"), Times.Once);
            _mediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mapper.Verify(mp => mp.Map<List<UserAutoCompleteDto>>(users), Times.Once);

            _repo.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
            _mediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenNoMatches_ReturnsEmptyList_AndPublishesAuditEvent()
        {
            // Arrange
            var q = new GetUserAutoCompleteQuery { SearchPattern = "zzz" };

            var users = new List<User>(); // repo returns none
            var mapped = new List<UserAutoCompleteDto>(); // mapper returns none

            _repo.Setup(r => r.GetUser("zzz")).ReturnsAsync(users);

            _mediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper
                .Setup(mp => mp.Map<List<UserAutoCompleteDto>>(users))
                .Returns(mapped);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(q, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Should().BeEmpty();

            _repo.Verify(r => r.GetUser("zzz"), Times.Once);
            _mediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mapper.Verify(mp => mp.Map<List<UserAutoCompleteDto>>(users), Times.Once);

            _repo.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
            _mediator.VerifyNoOtherCalls();
        }
    }
}
