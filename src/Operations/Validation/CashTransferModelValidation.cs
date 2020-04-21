using FluentValidation;
using Operations.DomainService.Model;

namespace Operations.Validation
{
    public class CashTransferModelValidation : AbstractValidator<CashTransferModel>
    {
        public CashTransferModelValidation()
        {
            RuleFor(o => o.Asset)
                .NotEmpty()
                .WithMessage("Asset required.")
                .MaximumLength(16)
                .WithMessage("Asset must be less than 16 characters.");

            RuleFor(o => o.Volume)
                .GreaterThan(0)
                .WithMessage("Volume must be greater then 0.");

            RuleFor(o => o.FromWallet)
                .NotEmpty()
                .WithMessage("Source wallet required.");

            RuleFor(o => o.ToWallet)
                .NotEmpty()
                .WithMessage("Target wallet required.");
        }
    }
}
