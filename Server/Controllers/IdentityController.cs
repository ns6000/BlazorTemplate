using BlazorTemplate.Shared.Routes;
using BlazorTemplate.Shared.Contracts;
using BlazorTemplate.Server.Services;

namespace BlazorTemplate.Server.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
	public readonly IdentityService identityService;

	public IdentityController(IdentityService identityService) =>
		this.identityService = identityService;

#if !DEBUG
	[Authorize]
#endif
	[HttpPost(Routes.Identity.Register)]
	public async Task<IActionResult> Register([FromBody]RegistrationRequest request)
	{
		ValidationResult validation = await new RegistrationRequestValidator().ValidateAsync(request);
		if(!validation.IsValid)
			return BadRequest(validation.Errors.Select(x => x.ErrorMessage));

		List<string> errors = await identityService.RegisterAsync(request);
		if(errors.Any())
			return BadRequest(errors.ToArray());
			
		return Ok();
	}

	[HttpPost(Routes.Identity.Login)]
	public async Task<IActionResult> Login([FromBody]LoginRequest request)
	{
		ValidationResult validation = await new LoginRequestValidator<LoginRequest>().ValidateAsync(request);
		if(!validation.IsValid)
			return BadRequest(new LoginResponse {
				Errors = validation.Errors.Select(x => x.ErrorMessage).ToArray()
			});

		LoginResponse loginResponse = await identityService.LoginAsync(request);
		if(!loginResponse.IsSuccessful)
			return BadRequest(loginResponse);
			
		return Ok(loginResponse);
	}
}