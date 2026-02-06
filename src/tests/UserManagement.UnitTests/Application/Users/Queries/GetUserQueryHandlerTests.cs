using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Users.Queries.GetUsers;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace UserManagement.UnitTests.Application.Users.Queries
{
    public class GetUserQueryHandlerTests
    {
        private readonly Mock<IUserQueryRepository> _repo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetUserQueryHandler>> _logger = new();

        private GetUserQueryHandler CreateSut()
            => new GetUserQueryHandler(_repo.Object, _mapper.Object, _mediator.Object, _logger.Object);

        [Fact]
        public async Task Handle_WhenUsersExist_ReturnsSuccess_WithMappedDtos_AndPublishesAuditEvent()
        {
            // Arrange
            var request = new GetUserQuery
            {
                PageNumber = 2,
                PageSize = 5,
                SearchTerm = "neo"
            };

            var users = new List<User>
            {
                new User { UserId = 1, UserName = "neo", FirstName = "Neo", LastName = "Anderson", EmailId = "neo@zion.io" },
                new User { UserId = 2, UserName = "trinity", FirstName = "Trinity", LastName = "Unknown", EmailId = "t@zion.io" },
            };
            var totalCount = 11;

            _repo.Setup(r => r.GetAllUsersAsync(request.PageNumber, request.PageSize, request.SearchTerm))
                 .ReturnsAsync((users, totalCount));

            var mapped = new List<UserDto>
            {
                new UserDto { UserId = 1, UserName = "neo", FirstName = "Neo", LastName = "Anderson", EmailId = "neo@zion.io" },
                new UserDto { UserId = 2, UserName = "trinity", FirstName = "Trinity", LastName = "Unknown", EmailId = "t@zion.io" },
            };

            _mapper.Setup(m => m.Map<List<UserDto>>(users))
                   .Returns(mapped);

            _mediator.Setup(m => m.Publish(
                        It.IsAny<AuditLogsDomainEvent>(),
                        It.IsAny<CancellationToken>()))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            ApiResponseDTO<List<UserDto>> result = await sut.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Be("Success");
            result.Data.Should().BeEquivalentTo(mapped);
            result.TotalCount.Should().Be(totalCount);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);

            _repo.Verify(r => r.GetAllUsersAsync(2, 5, "neo"), Times.Once);
            _mapper.Verify(m => m.Map<List<UserDto>>(users), Times.Once);
            _mediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);

            _repo.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
            _mediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_WhenNoUsers_ReturnsFailure_AndDoesNotPublishAuditEvent()
        {
            // Arrange
            var request = new GetUserQuery
            {
                PageNumber = 1,
                PageSize = 10,
                SearchTerm = null
            };

            var users = new List<User>(); // empty
            var totalCount = 0;

            _repo.Setup(r => r.GetAllUsersAsync(request.PageNumber, request.PageSize, request.SearchTerm))
                 .ReturnsAsync((users, totalCount));

            var sut = CreateSut();

            // Act
            ApiResponseDTO<List<UserDto>> result = await sut.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Be("No users found");
            result.Data.Should().BeNull();

            _repo.Verify(r => r.GetAllUsersAsync(1, 10, null), Times.Once);
            _mapper.VerifyNoOtherCalls();
            _mediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);

            _repo.VerifyNoOtherCalls();
        }
    }
}
