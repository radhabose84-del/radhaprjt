using AutoMapper;
using Contracts.Common;
using MediatR;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartMaster;
using Xunit;

namespace PartyManagement.UnitTests.Application.PartyMaster.Queries
{
    public sealed class GetAllPartyMasterQueryHandlerTests
    {
        private readonly Mock<IPartyMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPartMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<GetPartyMasterDto>
            {
                new GetPartyMasterDto { Id = 1, PartyCode = "PAR001", PartyName = "Test Party" }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllPartyMasterAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetPartyMasterDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetPartMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<GetPartyMasterDto>
            {
                new GetPartyMasterDto { Id = 1, PartyCode = "PAR001", PartyName = "Test Party" }
            };

            _mockQueryRepo
                .Setup(r => r.GetAllPartyMasterAsync(2, 5, "test"))
                .ReturnsAsync((dtoList, 11));

            _mockMapper
                .Setup(m => m.Map<List<GetPartyMasterDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetPartMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyList = new List<GetPartyMasterDto>();

            _mockQueryRepo
                .Setup(r => r.GetAllPartyMasterAsync(1, 10, null))
                .ReturnsAsync((emptyList, 0));

            _mockMapper
                .Setup(m => m.Map<List<GetPartyMasterDto>>(It.IsAny<object>()))
                .Returns(emptyList);

            var result = await CreateSut().Handle(
                new GetPartMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
