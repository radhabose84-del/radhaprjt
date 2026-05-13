using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureById;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureByUserId;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UserSignature.Queries
{
    public sealed class GetUserSignatureByUserIdQueryHandlerTests
    {
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUserSignatureByUserIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_SignatureExists_ReturnsDto()
        {
            var entity = UserSignatureBuilders.ValidEntityWithUser(1, 10);
            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByUserIdAsync(10))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserSignatureByIdDto>(entity))
                .Returns(new UserSignatureByIdDto { Id = 1, UserId = 10 });

            var result = await CreateSut().Handle(
                new GetUserSignatureByUserIdQuery { UserId = 10 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.UserId.Should().Be(10);
        }

        [Fact]
        public async Task Handle_NoSignatureForUser_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByUserIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.UserSignature?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetUserSignatureByUserIdQuery { UserId = 999 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }
    }
}
