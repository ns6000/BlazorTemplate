using FluentValidation;


namespace BlazorTemplate.Shared.Contracts;

public class LoginRequest
{
	public string? Login					{ get; set; }
	public string? Password					{ get; set; }
}

public class LoginRequestValidator<T> : AbstractValidator<T> where T : LoginRequest
{
	public LoginRequestValidator()
	{
		RuleFor(x => x.Login)
			.NotEmpty();
		RuleFor(x => x.Password)
			.NotEmpty();
	}
}



public class RegistrationRequest : LoginRequest
{
	public string? PasswordVerify			{ get; set; }
	public string? Email					{ get; set; }
}

public class RegistrationRequestValidator : LoginRequestValidator<RegistrationRequest>
{
	public RegistrationRequestValidator()
	{
		RuleFor(x => x.PasswordVerify)
			.NotEmpty()
			.Equal(x => x.Password).WithMessage($"Value of {nameof(RegistrationRequest.PasswordVerify)} must be equal to the value of {nameof(LoginRequest.Password)}");
		RuleFor(x => x.Email)
			.NotEmpty();
	}
}