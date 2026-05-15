using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Queries.GetAllUserSignature;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UserSignature.Queries
{
    public sealed class GetAllUserSignatureQueryHandlerTests
    {
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllUserSignatureQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithData_ReturnsSuccessResponse()
        {
            var entities = new List<UserManagement.Domain.Entities.UserSignature>
            {
                UserSignatureBuilders.ValidEntityWithUser(1, 10)
            };
            _mockQueryRepo
                .Setup(r => r.GetAllUserSignatureAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetAllUserSignatureDto>>(entities))
                .Returns(new List<GetAllUserSignatureDto> { new() { Id = 1, UserId = 10 } });

            var result = await CreateSut().Handle(
                new GetAllUserSignatureQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailureResponse()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllUserSignatureAsync(1, 10, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.UserSignature>(), 0));

            var result = await CreateSut().Handle(
                new GetAllUserSignatureQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No Record");
        }

        [Fact]
        public async Task Handle_WithData_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.UserSignature>
            {
                UserSignatureBuilders.ValidEntityWithUser(1, 10)
            };
            _mockQueryRepo
                .Setup(r => r.GetAllUserSignatureAsync(2, 5, "search"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<GetAllUserSignatureDto>>(entities))
                .Returns(new List<GetAllUserSignatureDto> { new() });

            var result = await CreateSut().Handle(
                new GetAllUserSignatureQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
