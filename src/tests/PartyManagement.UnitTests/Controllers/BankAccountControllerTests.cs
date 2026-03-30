using MediatR;
using Microsoft.AspNetCore.Mvc;
using PartyManagement.Application.BankAccount;
using PartyManagement.Application.BankAccount.Command.CreateBankAccount;
using PartyManagement.Application.BankAccount.Command.DeleteBankAccount;
using PartyManagement.Application.BankAccount.Command.UpdateBankAccount;
using PartyManagement.Application.BankAccount.Query.GetBankAccountById;
using PartyManagement.Application.BankAccount.Query.GetBankAccountsPaged;
using PartyManagement.Application.BankAccount.Query.GetBankAutocomplete;
using PartyManagement.Presentation.Controllers;
using PartyManagement.UnitTests.TestData;

namespace PartyManagement.UnitTests.Controllers
{
    public sealed class BankAccountControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private BankAccountController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            var items = (IReadOnlyList<BankAccountDto>)new List<BankAccountDto>();
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllBankAccountsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 0));

            var result = await CreateSut().GetAllBankAccountsAsync(1, 10, null, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsSenderOnce()
        {
            var items = (IReadOnlyList<BankAccountDto>)new List<BankAccountDto>();
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllBankAccountsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 0));

            await CreateSut().GetAllBankAccountsAsync(1, 10, null, null);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetAllBankAccountsQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ExistingId_ReturnsOkResult()
        {
            var dto = new BankAccountDto { Id = 1, AccountNumber = "1234567890", AccountHolderName = "Test Holder" };
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetBankAccountByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);

            var result = await CreateSut().GetById(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_NotFound_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetBankAccountByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BankAccountDto?)null);

            var result = await CreateSut().GetById(9999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsCreatedResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateBankAccountCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().Create(BankAccountBuilders.ValidCreateCommand());

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Update_Success_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateBankAccountCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Update(BankAccountBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_NotFound_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateBankAccountCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Update(BankAccountBuilders.ValidUpdateCommand());

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Delete_Success_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteBankAccountCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_NotFound_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteBankAccountCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Delete(9999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            var items = (IReadOnlyList<BankLookupDto>)new List<BankLookupDto>();
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetBankAccountAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);

            var result = await CreateSut().GetAllAutocomplete("test", CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
