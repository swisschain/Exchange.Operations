﻿using FluentValidation;
using JetBrains.Annotations;
using Operations.DomainService.Model;

namespace Operations.Validation
{
    [UsedImplicitly]
    public class CashTransferModelValidator : AbstractValidator<CashTransferModel>
    {
        public CashTransferModelValidator()
        {
            RuleFor(o => o.Asset)
                .NotEmpty()
                .WithMessage("Asset required.")
                .MaximumLength(16)
                .WithMessage("Asset must be less than 16 characters.");

            RuleFor(o => o.Volume)
                .GreaterThan(0)
                .WithMessage("Volume must be greater then 0.");

            RuleFor(o => o.FromWalletId)
                .NotEmpty()
                .WithMessage("Source wallet required.");

            RuleFor(o => o.ToWalletId)
                .NotEmpty()
                .WithMessage("Target wallet required.");
        }
    }
}
