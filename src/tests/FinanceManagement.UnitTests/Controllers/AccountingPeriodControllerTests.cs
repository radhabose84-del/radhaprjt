using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers.JournalMaster;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.CreateAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.UpdateAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.DeleteAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAllAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodById;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Queries.GetAccountingPeriodAutoComplete;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class AccountingPeriodControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private AccountingPeriodController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllAccountingPeriodQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AccountingPeriodDto>>
                {
                    IsSuccess = true,
                    Data = new List<AccountingPeriodDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAccountingPeriodAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllAccountingPeriodQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AccountingPeriodDto>> { IsSuccess = true, Data = new List<AccountingPeriodDto>() });

            await CreateSut().GetAllAccountingPeriodAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllAccountingPeriodQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccountingPeriodByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(AccountingPeriodBuilders.ValidDto());

            var result = await CreateSut().GetAccountingPeriodByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAccountingPeriodAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<AccountingPeriodLookupDto>)AccountingPeriodBuilders.ValidLookupList());

            var result = await CreateSut().GetAccountingPeriodAutoCompleteAsync("Jun");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAccountingPeriodCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateAccountingPeriod(AccountingPeriodBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAccountingPeriodCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateAccountingPeriod(AccountingPeriodBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAccountingPeriodCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAccountingPeriod(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteAccountingPeriodCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAccountingPeriod(1);

            _mockMediator.Verify(m => m.Send(It.IsAny<DeleteAccountingPeriodCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
