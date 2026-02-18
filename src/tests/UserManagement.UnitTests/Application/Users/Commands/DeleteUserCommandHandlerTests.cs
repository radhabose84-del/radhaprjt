using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Users.Commands.DeleteUser;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserManagement.UnitTests.Application.Users.Commands
{
    public class DeleteUserCommandHandlerTests
    {
        private readonly Mock<IUserCommandRepository> _cmdRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<DeleteUserCommandHandler>> _logger = new(MockBehavior.Loose);

        private DeleteUserCommandHandler CreateSut()
            => new(_cmdRepo.Object, _mapper.Object, _mediator.Object, _logger.Object);

        private static DeleteUserCommand MakeRequest() => new DeleteUserCommand
        {
            UserId = 7,
            // add any other props your command exposes
        };

        private static User MakeMappedUser(DeleteUserCommand req) => new User
        {
            UserId = req.UserId,
        };

        [Fact]
        public async Task Handle_WhenRepositoryDeletes_ReturnsSuccess_AndPublishesAuditEvent()
        {
            // Arrange
            var request = MakeRequest();
            var mapped = MakeMappedUser(request);

            _mapper
                .Setup(m => m.Map<User>(request))
                .Returns(mapped);

            _mediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _cmdRepo
                .Setup(r => r.DeleteAsync(request.UserId, mapped))
                .ReturnsAsync(true);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<bool> result = await sut.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be("User deleted successfully.");

            // Verify interactions
            _mapper.VerifyAll();
            _cmdRepo.VerifyAll();
            _mediator.Verify(m =>
                m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenRepositoryReturnsFalse_ReturnsFailure_AndStillPublishesAuditEvent()
        {
            // Arrange
            var request = MakeRequest();
            var mapped = MakeMappedUser(request);

            _mapper
                .Setup(m => m.Map<User>(request))
                .Returns(mapped);

            // Domain event is published before calling repo
            _mediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _cmdRepo
                .Setup(r => r.DeleteAsync(request.UserId, mapped))
                .ReturnsAsync(false);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeFalse();
            result.Message.Should().Be("User could not be deleted.");

            _mapper.VerifyAll();
            _cmdRepo.VerifyAll();
            _mediator.Verify(m =>
                m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
