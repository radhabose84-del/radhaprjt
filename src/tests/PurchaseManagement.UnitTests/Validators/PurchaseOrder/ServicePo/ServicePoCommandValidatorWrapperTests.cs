using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Update;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.ServicePo
{
    /// <summary>
    /// The command wrappers exist so MediatR's ValidationBehavior (which only resolves
    /// IValidator&lt;TCommand&gt;) actually triggers the DTO-level rules. These tests cover the
    /// wrapper itself — that NotNull on Data is enforced, and that SetValidator delegates
    /// to the inner DTO validator.
    /// </summary>
    public sealed class ServicePoCommandValidatorWrapperTests
    {
        // --- Create wrapper ---

        [Fact]
        public async Task Create_NullData_FailsWith_RequiredBody()
        {
            var inner = new Mock<IValidator<CreateServicePurchaseOrderDto>>(MockBehavior.Loose);
            var sut = new CreateServicePoCommandValidator(inner.Object);

            // Bypass the C# `required` keyword via reflection / a null-forgiving inline command
            var command = new CreateServicePoCommand { Data = null! };

            var result = await sut.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Data)
                  .WithErrorMessage("Request body is required.");
        }

        [Fact]
        public async Task Create_NonNullData_DelegatesTo_InnerDtoValidator()
        {
            var inner = new Mock<IValidator<CreateServicePurchaseOrderDto>>(MockBehavior.Strict);
            inner.Setup(v => v.ValidateAsync(
                    It.IsAny<ValidationContext<CreateServicePurchaseOrderDto>>(),
                    It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ValidationResult());

            var sut = new CreateServicePoCommandValidator(inner.Object);
            var command = new CreateServicePoCommand { Data = new CreateServicePurchaseOrderDto() };

            await sut.TestValidateAsync(command);

            inner.Verify(v => v.ValidateAsync(
                    It.IsAny<ValidationContext<CreateServicePurchaseOrderDto>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- Update wrapper ---

        [Fact]
        public async Task Update_NullData_FailsWith_RequiredBody()
        {
            var inner = new Mock<IValidator<CreateServicePurchaseOrderDto>>(MockBehavior.Loose);
            var sut = new UpdateServicePoCommandValidator(inner.Object);

            var command = new UpdateServicePoCommand { Data = null! };

            var result = await sut.TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Data)
                  .WithErrorMessage("Request body is required.");
        }

        [Fact]
        public async Task Update_DataWithoutId_FailsWith_IdRequired()
        {
            var inner = new Mock<IValidator<CreateServicePurchaseOrderDto>>(MockBehavior.Loose);
            inner.Setup(v => v.ValidateAsync(
                    It.IsAny<ValidationContext<CreateServicePurchaseOrderDto>>(),
                    It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ValidationResult());

            var sut = new UpdateServicePoCommandValidator(inner.Object);
            var command = new UpdateServicePoCommand
            {
                Data = new CreateServicePurchaseOrderDto { Id = 0 }
            };

            var result = await sut.TestValidateAsync(command);

            result.Errors.Should()
                .Contain(e => e.ErrorMessage.Contains("Service PO Id is required for update"));
        }

        [Fact]
        public async Task Update_DataWithValidId_DelegatesTo_InnerDtoValidator()
        {
            var inner = new Mock<IValidator<CreateServicePurchaseOrderDto>>(MockBehavior.Strict);
            inner.Setup(v => v.ValidateAsync(
                    It.IsAny<ValidationContext<CreateServicePurchaseOrderDto>>(),
                    It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new ValidationResult());

            var sut = new UpdateServicePoCommandValidator(inner.Object);
            var command = new UpdateServicePoCommand
            {
                Data = new CreateServicePurchaseOrderDto { Id = 123 }
            };

            await sut.TestValidateAsync(command);

            inner.Verify(v => v.ValidateAsync(
                    It.IsAny<ValidationContext<CreateServicePurchaseOrderDto>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
