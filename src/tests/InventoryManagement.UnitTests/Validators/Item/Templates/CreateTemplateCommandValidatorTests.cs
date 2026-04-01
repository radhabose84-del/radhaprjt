using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.CreateTemplate;
using InventoryManagement.Application.Item.Templates.DTOs;
using InventoryManagement.Presentation.Validation.Item.Templates;

namespace InventoryManagement.UnitTests.Validators.Item.Templates
{
    public sealed class CreateTemplateCommandValidatorTests
    {
        private readonly Mock<ITemplateQueryRepository> _mockRepo = new(MockBehavior.Loose);

        public CreateTemplateCommandValidatorTests()
        {
            _mockRepo
                .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        private CreateTemplateCommandValidator CreateValidator() => new(_mockRepo.Object);

        private static CreateTemplateCommand ValidCommand() => new(
            TemplateName: "Quality Inspection Template",
            Parameters: new List<TemplateParameterDto>
            {
                new TemplateParameterDto { Parameter = "Hardness", Numeric = false }
            }
        );

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTemplateName_FailsValidation(string? name)
        {
            var command = new CreateTemplateCommand(TemplateName: name!, Parameters: null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateTemplateName_FailsValidation()
        {
            _mockRepo
                .Setup(r => r.ExistsByNameAsync("Quality Inspection Template", null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NumericParamWithoutMinMax_FailsValidation()
        {
            var command = new CreateTemplateCommand(
                TemplateName: "Template A",
                Parameters: new List<TemplateParameterDto>
                {
                    new TemplateParameterDto { Parameter = "Tensile", Numeric = true, MinimumValue = null, MaximumValue = null }
                }
            );

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NumericParamWithMaxLessThanMin_FailsValidation()
        {
            var command = new CreateTemplateCommand(
                TemplateName: "Template B",
                Parameters: new List<TemplateParameterDto>
                {
                    new TemplateParameterDto { Parameter = "Weight", Numeric = true, MinimumValue = 100m, MaximumValue = 10m }
                }
            );

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NullParameters_PassesValidation()
        {
            var command = new CreateTemplateCommand(TemplateName: "Simple Template", Parameters: null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
