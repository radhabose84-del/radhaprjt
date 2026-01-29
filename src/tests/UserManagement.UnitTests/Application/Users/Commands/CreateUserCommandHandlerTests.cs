using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.IUser;
using Core.Application.Users.Commands.CreateUser;
using Core.Application.Users.Queries.GetUsers;
using Core.Domain.Entities;
using Core.Domain.Events;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserManagement.UnitTests.Application.Users.Commands
{
    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUserCommandRepository> _mockUserCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<CreateUserCommandHandler>> _mockLogger = new();
        private readonly Mock<IEventPublisher> _mockEventPublisher = new(MockBehavior.Strict);

        private CreateUserCommandHandler CreateSut() =>
            new CreateUserCommandHandler(
                _mockUserCommandRepo.Object,
                _mockMapper.Object,
                _mockMediator.Object,
                _mockLogger.Object,
                _mockEventPublisher.Object);

        [Fact]
        public async Task Handle_Success_Flows_All_Steps_And_Returns_Success()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                UserName = "testuser",
                EmailId = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Mobile = "1234567890"
            };

            var userEntity = new User
            {
                UserName = "testuser",
                EmailId = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Mobile = "1234567890"
            };

            var createdUser = new User
            {
                UserId = 1,
                UserName = "testuser",
                EmailId = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Mobile = "1234567890"
            };

            var userDto = new UserDto
            {
                UserId = 1,
                UserName = "testuser",
                EmailId = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Mobile = "1234567890"
            };

            _mockMapper.Setup(x => x.Map<User>(command)).Returns(userEntity);

            _mockUserCommandRepo.Setup(x => x.GetMiscmasterByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(1);

            _mockUserCommandRepo.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            _mockMediator.Setup(x => x.Publish(
                    It.IsAny<AuditLogsDomainEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockEventPublisher.Setup(x => x.PublishPendingEventsAsync())
                .Returns(Task.CompletedTask);

            _mockMapper.Setup(x => x.Map<UserDto>(createdUser)).Returns(userDto);

            var handler = CreateSut();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("User created successfully");
            result.Data.Should().NotBeNull();
            result.Data!.UserId.Should().Be(1);
            result.Data.UserName.Should().Be("testuser");

            _mockEventPublisher.Verify(x => x.PublishPendingEventsAsync(), Times.Once);

            _mockUserCommandRepo.VerifyAll();
            _mockMapper.VerifyAll();
            _mockMediator.VerifyAll();
            _mockEventPublisher.VerifyAll();
        }

        [Fact]
        public async Task Handle_When_CreateAsync_ReturnsNull_Returns_Failure()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                UserName = "testuser",
                EmailId = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var userEntity = new User
            {
                UserName = "testuser",
                EmailId = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            _mockMapper.Setup(x => x.Map<User>(command)).Returns(userEntity);

            _mockUserCommandRepo.Setup(x => x.GetMiscmasterByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(1);

            _mockUserCommandRepo.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User)null!);

            var handler = CreateSut();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("Failed to create user. Please try again.");
            result.Data.Should().BeNull();

            _mockUserCommandRepo.VerifyAll();
            _mockMapper.VerifyAll();
        }

        [Fact]
        public async Task Handle_Should_Map_Command_To_User_Entity()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                UserName = "john.doe",
                EmailId = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                Mobile = "9876543210"
            };

            var userEntity = new User
            {
                UserName = "john.doe",
                EmailId = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                Mobile = "9876543210"
            };

            var createdUser = new User
            {
                UserId = 5,
                UserName = "john.doe",
                EmailId = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                Mobile = "9876543210"
            };

            var userDto = new UserDto
            {
                UserId = 5,
                UserName = "john.doe",
                EmailId = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                Mobile = "9876543210"
            };

            _mockMapper.Setup(x => x.Map<User>(command)).Returns(userEntity);

            _mockUserCommandRepo.Setup(x => x.GetMiscmasterByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(1);

            _mockUserCommandRepo.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            _mockMediator.Setup(x => x.Publish(
                    It.IsAny<AuditLogsDomainEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockEventPublisher.Setup(x => x.PublishPendingEventsAsync())
                .Returns(Task.CompletedTask);

            _mockMapper.Setup(x => x.Map<UserDto>(createdUser)).Returns(userDto);

            var handler = CreateSut();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.UserName.Should().Be("john.doe");
            result.Data.EmailId.Should().Be("john@example.com");

            _mockMapper.Verify(x => x.Map<User>(command), Times.Once);
            _mockMapper.Verify(x => x.Map<UserDto>(createdUser), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Publish_AuditLog_DomainEvent()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                UserName = "audit.user",
                EmailId = "audit@example.com",
                FirstName = "Audit",
                LastName = "User",
                Mobile = "1112223333"
            };

            var userEntity = new User
            {
                UserName = "audit.user",
                EmailId = "audit@example.com",
                FirstName = "Audit",
                LastName = "User",
                Mobile = "1112223333"
            };

            var createdUser = new User
            {
                UserId = 10,
                UserName = "audit.user",
                EmailId = "audit@example.com",
                FirstName = "Audit",
                LastName = "User",
                Mobile = "1112223333"
            };

            var userDto = new UserDto
            {
                UserId = 10,
                UserName = "audit.user",
                EmailId = "audit@example.com",
                FirstName = "Audit",
                LastName = "User",
                Mobile = "1112223333"
            };

            _mockMapper.Setup(x => x.Map<User>(command)).Returns(userEntity);

            _mockUserCommandRepo.Setup(x => x.GetMiscmasterByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(1);

            _mockUserCommandRepo.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            _mockMediator.Setup(x => x.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.Module == "User" &&
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "audit.user"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockEventPublisher.Setup(x => x.PublishPendingEventsAsync())
                .Returns(Task.CompletedTask);

            _mockMapper.Setup(x => x.Map<UserDto>(createdUser)).Returns(userDto);

            var handler = CreateSut();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _mockMediator.Verify(x => x.Publish(
                It.Is<AuditLogsDomainEvent>(e =>
                    e.Module == "User" &&
                    e.ActionDetail == "Create" &&
                    e.ActionCode == "audit.user"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Call_PublishPendingEventsAsync()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                UserName = "event.user",
                EmailId = "event@example.com",
                FirstName = "Event",
                LastName = "User",
                Mobile = "4445556666"
            };

            var userEntity = new User
            {
                UserName = "event.user",
                EmailId = "event@example.com",
                FirstName = "Event",
                LastName = "User",
                Mobile = "4445556666"
            };

            var createdUser = new User
            {
                UserId = 15,
                UserName = "event.user",
                EmailId = "event@example.com",
                FirstName = "Event",
                LastName = "User",
                Mobile = "4445556666"
            };

            var userDto = new UserDto
            {
                UserId = 15,
                UserName = "event.user",
                EmailId = "event@example.com",
                FirstName = "Event",
                LastName = "User",
                Mobile = "4445556666"
            };

            _mockMapper.Setup(x => x.Map<User>(command)).Returns(userEntity);

            _mockUserCommandRepo.Setup(x => x.GetMiscmasterByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(1);

            _mockUserCommandRepo.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            _mockMediator.Setup(x => x.Publish(
                    It.IsAny<AuditLogsDomainEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockEventPublisher.Setup(x => x.PublishPendingEventsAsync())
                .Returns(Task.CompletedTask);

            _mockMapper.Setup(x => x.Map<UserDto>(createdUser)).Returns(userDto);

            var handler = CreateSut();

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _mockEventPublisher.Verify(x => x.PublishPendingEventsAsync(), Times.Once);
        }
    }
}