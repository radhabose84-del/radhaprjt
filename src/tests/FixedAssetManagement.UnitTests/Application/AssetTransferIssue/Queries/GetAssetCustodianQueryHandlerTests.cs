using AutoMapper;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetCustodian;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FluentValidation;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.AssetTransferIssue.Queries
{
    public sealed class GetAssetCustodianQueryHandlerTests
    {
        private readonly Mock<IAssetTransferQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAssetCustodianQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidRequest_ReturnsCustodianList()
        {
            var dtos = new List<GetAssetCustodianDto> { new() };

            _mockRepo
                .Setup(r => r.GetCustodianByDepartmentAsync("UNIT1", 1))
                .ReturnsAsync(dtos);

            var result = await CreateSut().Handle(
                new GetAssetCustodianQuery { OldUnitId = "UNIT1", DepartmentId = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyOldUnitId_ThrowsValidationException()
        {
            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetCustodianQuery { OldUnitId = "", DepartmentId = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_NullOldUnitId_ThrowsValidationException()
        {
            Func<Task> act = async () => await CreateSut().Handle(
                new GetAssetCustodianQuery { OldUnitId = null, DepartmentId = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
