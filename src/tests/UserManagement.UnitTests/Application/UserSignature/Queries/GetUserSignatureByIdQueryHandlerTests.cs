using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureById;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UserSignature.Queries
{
    public sealed class GetUserSignatureByIdQueryHandlerTests
    {
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IUserSignatureFileStorage> _mockFileStorage = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUserSignatureByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockFileStorage.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_RecordExists_ReturnsDtoWithBase64()
        {
            var entity = UserSignatureBuilders.ValidEntityWithUser(5, 10);
            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserSignatureByIdDto>(entity))
                .Returns(new UserSignatureByIdDto { Id = 5, UserId = 10, FileName = entity.FileName });

            _mockFileStorage
                .Setup(s => s.ReadAsBase64Async(entity.FilePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync("ZmFrZS1iYXNlNjQ=");

            var result = await CreateSut().Handle(
                new GetUserSignatureByIdQuery { Id = 5 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(5);
            result.ImageBase64.Should().Be("ZmFrZS1iYXNlNjQ=");
        }

        [Fact]
        public async Task Handle_RecordNotFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(99))
                .ReturnsAsync((UserManagement.Domain.Entities.UserSignature?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetUserSignatureByIdQuery { Id = 99 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_FileMissingOnDisk_ReturnsDtoWithNullBase64()
        {
            var entity = UserSignatureBuilders.ValidEntityWithUser(5, 10);
            _mockQueryRepo
                .Setup(r => r.GetUserSignatureByIdAsync(5))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<UserSignatureByIdDto>(entity))
                .Returns(new UserSignatureByIdDto { Id = 5, UserId = 10, FileName = entity.FileName });

            _mockFileStorage
                .Setup(s => s.ReadAsBase64Async(entity.FilePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var result = await CreateSut().Handle(
                new GetUserSignatureByIdQuery { Id = 5 },
                CancellationToken.None);

            result.ImageBase64.Should().BeNull();
        }
    }
}
