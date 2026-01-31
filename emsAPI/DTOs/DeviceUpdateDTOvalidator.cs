using FluentValidation;

namespace Models.Dtos.Device;

public class DeviceUpdateDtoValidator : AbstractValidator<DeviceUpdateDto>
{
    public DeviceUpdateDtoValidator()
    {
        RuleFor(x => x.SerialNumber)
            .NotEmpty()
            .WithMessage("Numer seryjny jest wymagany.")
            .MaximumLength(100)
            .WithMessage("Numer seryjny może mieć maksymalnie 100 znaków.")
            .Must(sn => !string.IsNullOrWhiteSpace(sn))
            .WithMessage("Numer seryjny nie może składać się tylko z białych znaków.")
            .Matches(@"^[A-Za-z0-9\-_]+$")
            .WithMessage("Numer seryjny może zawierać tylko litery, cyfry, myślniki i podkreślenia.");

        RuleFor(x => x.TypeId)
            .GreaterThan(0)
            .WithMessage("Typ urządzenia jest wymagany.");

        RuleFor(x => x.ProducerId)
            .GreaterThan(0)
            .WithMessage("Producent jest wymagany.");
    }
}