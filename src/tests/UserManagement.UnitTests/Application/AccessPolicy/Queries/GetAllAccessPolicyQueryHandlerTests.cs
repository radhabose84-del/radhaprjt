using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.AccessPolicy.Dto;
using UserManagement.Application.AccessPolicy.Queries.GetAllAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.AccessPolicy.Queries
{
    public sealed class GetAllAccessPolicyQueryHandlerTests
    {
        private readonly Mock<IAccessPolicyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper>                      _mockMapper    = new(MockBehavior.Loose);
        private readonly Mock<IMediator>                    _mockMediator  = new(MockBehavior.Loose);

        private GetAllAccessPolicyQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<AccessPolicyDto> { AccessPolicyBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllAccessPolicyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsData()
        {
            var dtoList = new List<AccessPolicyDto> { AccessPolicyBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllAccessPolicyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().HaveCount(1);
            result.Data![0].PolicyCode.Should().Be("AP001");
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<AccessPolicyDto> { AccessPolicyBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "test"))
                .ReturnsAsync((dtoList, 11));

            var result = await CreateSut().Handle(
                new GetAllAccessPolicyQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<AccessPolicyDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllAccessPolicyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<AccessPolicyDto>(), 0));

            await CreateSut().Handle(
                new GetAllAccessPolicyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, null), Times.Once);
        }
    }
}
