using Contracts.Common;
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackNoValidation;


namespace SalesManagement.UnitTests.Application.DispatchAdvice.Queries
{
    public sealed class GetDispatchAdvicePackNoValidationQueryHandlerTests
    {
        private readonly Mock<IDispatchAdviceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDispatchAdvicePackNoValidationQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMiscRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_AllPacksAvailable_ReturnsValid()
        {
            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockQueryRepo
                .Setup(r => r.GetAvailablePackNosAsync(1, 1, 1, 1, 3, 1))
                .ReturnsAsync(new List<int> { 1, 2, 3 });

            var result = await CreateSut().Handle(
                new GetDispatchAdvicePackNoValidationQuery
                {
                    ItemId = 1, LotId = 1, StartPackNo = 1, EndPackNo = 3, PackTypeId = 1
                }, CancellationToken.None);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_MissingPacks_ReturnsInvalid()
        {
            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockQueryRepo
                .Setup(r => r.GetAvailablePackNosAsync(1, 1, 1, 1, 3, 1))
                .ReturnsAsync(new List<int> { 1, 3 }); // Pack 2 missing

            var result = await CreateSut().Handle(
                new GetDispatchAdvicePackNoValidationQuery
                {
                    ItemId = 1, LotId = 1, StartPackNo = 1, EndPackNo = 3, PackTypeId = 1
                }, CancellationToken.None);

            result.IsValid.Should().BeFalse();
            result.MissingPackNos.Should().Contain(2);
        }
    }
}
