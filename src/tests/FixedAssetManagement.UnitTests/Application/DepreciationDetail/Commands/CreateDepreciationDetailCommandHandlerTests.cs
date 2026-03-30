using AutoMapper;
using Contracts.Common;
using FAM.Application.Common.Interfaces.IDepreciationDetail;
using FAM.Application.DepreciationDetail.Commands.CreateDepreciationDetail;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;

namespace FixedAssetManagement.UnitTests.Application.DepreciationDetail.Commands
{
    public sealed class CreateDepreciationDetailCommandHandlerTests
    {
        private readonly Mock<IDepreciationDetailQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateDepreciationDetailCommandHandler CreateSut() =>
            new(_mockMapper.Object, _mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_StatusCode1_ReturnsMessage()
        {
            _mockQueryRepo
                .Setup(r => r.CreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(("Depreciation created successfully.", 1));

            var result = await CreateSut().Handle(
                DepreciationDetailBuilders.ValidCreateCommand(),
                CancellationToken.None);

            result.Should().Be("Depreciation created successfully.");
        }

        [Fact]
        public async Task Handle_StatusCodeMinus1_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.CreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(("Depreciation already exists.", -1));

            await Assert.ThrowsAsync<ExceptionRules>(() =>
                CreateSut().Handle(DepreciationDetailBuilders.ValidCreateCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UnknownStatusCode_ThrowsExceptionRules()
        {
            _mockQueryRepo
                .Setup(r => r.CreateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(("Some unexpected message.", 99));

            await Assert.ThrowsAsync<ExceptionRules>(() =>
                CreateSut().Handle(DepreciationDetailBuilders.ValidCreateCommand(), CancellationToken.None));
        }
    }
}
