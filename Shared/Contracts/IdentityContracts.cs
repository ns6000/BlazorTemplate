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

public class LoginResponse
{
	public bool IsSuccessful				{ get; set; }
	public string? Token					{ get; set; }
	public string[] Errors					{ get; set; } = Array.Empty<string>();
}



public class RegistrationRequest : LoginRequest
{
	public string? PasswordVerification		{ get; set; }
	public string? Email					{ get; set; }
}

public class RegistrationRequestValidator : LoginRequestValidator<RegistrationRequest>
{
	public RegistrationRequestValidator()
	{
		RuleFor(x => x.PasswordVerification)
			.NotEmpty()
			.Equal(x => x.Password);
		RuleFor(x => x.Email)
			.NotEmpty();
	}
}