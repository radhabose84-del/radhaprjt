using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Users.Queries.GetUserById;
using Core.Application.Users.Queries.GetUsers; // for UserByIdDTO if it lives here; adjust if different
using Core.Domain.Entities;
using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using Core.Application.Common.Interfaces.IUser;

namespace UserManagement.UnitTests.Application.Users.Queries
{
    public class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<IUserQueryRepository> _repo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetUserByIdQueryHandler>> _logger = new();

        private GetUserByIdQueryHandler CreateSut()
            => new GetUserByIdQueryHandler(_repo.Object, _mapper.Object, _mediator.Object, _logger.Object);

        [Fact]
        public async Task Handle_WhenUserExists_MapsAndPublishesAuditEvent_ReturnsDto()
        {
            // Arrange
            var query = new GetUserByIdQuery { UserId = 7 };

            var user = new User
            {
                UserId = 7,
                UserName = "neo",
                FirstName = "Neo",
                LastName = "Anderson",
                EmailId = "neo@zion.io"
            };

            var dto = new UserByIdDTO
            {
                UserId = 7,
                UserName = "neo",
                FirstName = "Neo",
                LastName = "Anderson",
                EmailId = "neo@zion.io"
            };

            _repo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(user);

            _mediator.Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.Module == "User" &&
                        e.ActionDetail == "GetById" &&
                        e.ActionCode == "neo" &&
                        e.ActionName == "Neo Anderson" &&
                        e.Details!.Contains("Fetched details for User 'neo'.")),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mapper.Setup(mp => mp.Map<UserByIdDTO>(user)).Returns(dto);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(7);
            result.UserName.Should().Be("neo");

            _repo.Verify(r => r.GetByIdAsync(7), Times.Once);
            _mediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mapper.Verify(mp => mp.Map<UserByIdDTO>(user), Times.Once);

            _repo.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
            _mediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenUserDoesNotExist_ReturnsNull_AndDoesNotPublishEventOrMap()
        {
            // Arrange
            var query = new GetUserByIdQuery { UserId = 99 };

            _repo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((User)null!);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();

            _repo.Verify(r => r.GetByIdAsync(99), Times.Once);
            _mediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
            _mapper.Verify(mp => mp.Map<UserByIdDTO>(It.IsAny<User>()), Times.Never);

            _repo.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
            _mediator.VerifyNoOtherCalls();
        }
    }
}
