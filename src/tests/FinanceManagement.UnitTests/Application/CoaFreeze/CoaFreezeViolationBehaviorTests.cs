using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.CoaFreeze.Commands.SetCoaFreezeState;
using FinanceManagement.Application.Common.Behaviors;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using MediatR;

namespace FinanceManagement.UnitTests.Application.CoaFreeze
{
    public sealed class CoaFreezeViolationBehaviorTests
    {
        private readonly Mock<ICoaFreezeViolationLog> _mockLog = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CoaFreezeViolationBehavior<SetCoaFreezeStateCommand, ApiResponseDTO<bool>> CreateSut() =>
            new(_mockLog.Object, _mockIp.Object);

        private static readonly SetCoaFreezeStateCommand Request = new() { IsFrozen = true };

        [Fact]
        public async Task Handle_NoError_PassesThrough_NoLog()
        {
            var expected = new ApiResponseDTO<bool> { IsSuccess = true, Data = true };
            RequestHandlerDelegate<ApiResponseDTO<bool>> next = () => Task.FromResult(expected);

            var result = await CreateSut().Handle(Request, next, CancellationToken.None);

            result.Should().BeSameAs(expected);
            _mockLog.Verify(l => l.LogAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_FreezeViolation_LogsAndThrowsFriendly()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUserId()).Returns(396);
            RequestHandlerDelegate<ApiResponseDTO<bool>> next =
                () => Task.FromException<ApiResponseDTO<bool>>(new Exception("...COA_FREEZE_VIOLATION..."));

            Func<Task> act = async () => await CreateSut().Handle(Request, next, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*frozen*");
            _mockLog.Verify(l => l.LogAsync(1, 396, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_UnrelatedError_Rethrown_NotCaught()
        {
            RequestHandlerDelegate<ApiResponseDTO<bool>> next =
                () => Task.FromException<ApiResponseDTO<bool>>(new InvalidOperationException("something else"));

            Func<Task> act = async () => await CreateSut().Handle(Request, next, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
            _mockLog.Verify(l => l.LogAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
