using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IUser;
using Core.Application.Users.Commands.UpdateUser;
using Core.Domain.Entities;
using Core.Domain.Events;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserManagement.UnitTests.Application.Users.Commands
{
    public class UpdateUserCommandHandlerTests
    {
        private readonly Mock<IUserCommandRepository> _cmdRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserQueryRepository> _qryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UpdateUserCommandHandler>> _logger = new(MockBehavior.Loose);

        private UpdateUserCommandHandler CreateSut() =>
            new(_cmdRepo.Object, _qryRepo.Object, _mapper.Object, _mediator.Object, _logger.Object);

        private static UpdateUserCommand MakeRequest() => new UpdateUserCommand
        {
            UserId = 10,
            UserName = "new.name",
            FirstName = "Neo",
            LastName = "Anderson"
        };

        private static User MakeExisting() => new User
        {
            UserId = 10,
            UserName = "old.name",
            FirstName = "Old",
            LastName = "Name",
            // whatever other defaults your entity has
        };

        [Fact]
        public async Task Handle_Success_Updates_User_And_Returns_Success_Response()
        {
            // Arrange
            var req = MakeRequest();
            var existing = MakeExisting();

            _qryRepo
                .Setup(r => r.GetByIdAsync(req.UserId))
                .ReturnsAsync(existing);

            // The handler assigns an int (or similar) returned from this call
            _cmdRepo
                .Setup(r => r.GetMiscmasterByIdAsync(
                    Core.Domain.Enums.Common.MiscEnumEntity.UserType.MiscTypeCode,
                    Core.Domain.Enums.Common.MiscEnumEntity.UserType.Internal))
                .ReturnsAsync(1);

            // IMPORTANT: Map(source, destination) must be set up and return the destination
            _mapper
                .Setup(m => m.Map(req, existing))
                .Returns(existing);

            _mediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _cmdRepo
                .Setup(r => r.UpdateAsync(req.UserId, existing))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<bool> result = await sut.Handle(req, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be("User updated successfully.");

            _qryRepo.VerifyAll();
            _cmdRepo.VerifyAll();
            _mapper.VerifyAll();
            _mediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_When_UpdateAsync_ReturnsZero_Returns_Failure_Response()
        {
            // Arrange
            var req = MakeRequest();
            var existing = MakeExisting();

            _qryRepo
                .Setup(r => r.GetByIdAsync(req.UserId))
                .ReturnsAsync(existing);

            _cmdRepo
                .Setup(r => r.GetMiscmasterByIdAsync(
                    Core.Domain.Enums.Common.MiscEnumEntity.UserType.MiscTypeCode,
                    Core.Domain.Enums.Common.MiscEnumEntity.UserType.Internal))
                .ReturnsAsync(1);

            _mapper
                .Setup(m => m.Map(req, existing))
                .Returns(existing);

            _mediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _cmdRepo
                .Setup(r => r.UpdateAsync(req.UserId, existing))
                .ReturnsAsync(0);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(req, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Data.Should().BeFalse();
            result.Message.Should().Be("User update failed.");

            _qryRepo.VerifyAll();
            _cmdRepo.VerifyAll();
            _mapper.VerifyAll();
            _mediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_When_User_Not_Found_Current_Code_Throws_NullReference()
        {
            // Arrange
            var req = MakeRequest();

            // Force "not found"
            _qryRepo
                .Setup(r => r.GetByIdAsync(req.UserId))
                .ReturnsAsync((User)null!);

            // Let the code pass the strict mock here so it can reach the null dereference:
            _cmdRepo
                .Setup(r => r.GetMiscmasterByIdAsync(
                    Core.Domain.Enums.Common.MiscEnumEntity.UserType.MiscTypeCode,
                    Core.Domain.Enums.Common.MiscEnumEntity.UserType.Internal))
                .ReturnsAsync(1);

            var sut = CreateSut();

            // Act / Assert
            // Because handler writes: existingUser = null; then existingUser.UserType = <value>; => NRE
            await FluentAssertions.FluentActions
                .Invoking(() => sut.Handle(req, CancellationToken.None))
                .Should().ThrowAsync<NullReferenceException>();
        }
    }
}
