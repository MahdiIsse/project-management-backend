using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.DTOs.Auth;
using ProjectManagement.Application.Interfaces.Repositories;
using ProjectManagement.Application.Interfaces.Services;

namespace ProjectManagement.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : BaseController
{
  private readonly UserManager<IdentityUser> _userManager;
  private readonly RoleManager<IdentityRole> _roleManager;
  private readonly ITokenRepository _tokenRepository;
  private readonly IOnboardingService _onboardingService;

  public AuthController(
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ITokenRepository tokenRepository,
    IOnboardingService onboardingService)
  {
    _userManager = userManager;
    _roleManager = roleManager;
    _tokenRepository = tokenRepository;
    _onboardingService = onboardingService;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
  {
    var user = new IdentityUser { UserName = registerRequestDto.Email, Email = registerRequestDto.Email };
    var result = await _userManager.CreateAsync(user, registerRequestDto.Password);

    if (!result.Succeeded) return BadRequest(result.Errors);

    if (!await _roleManager.RoleExistsAsync("User"))
      await _roleManager.CreateAsync(new IdentityRole("User"));

    await _userManager.AddToRoleAsync(user, "User");

    var (onboardingSuccess, onboardingError) = await _onboardingService.CreateInitialDataAsync(user.Id);

    var roles = await _userManager.GetRolesAsync(user);
    var (token, expires) = await _tokenRepository.CreateTokenAsync(user, roles);

    var response = new RegisterResponseDto
    {
      Token = token,
      ExpiresAtUtc = expires,
      OnboardingCompleted = onboardingSuccess,
      OnboardingError = onboardingError,
      Message = onboardingSuccess
        ? "User registered and onboarded successfully"
        : $"User registered but onboarding failed: {onboardingError}"
    };

    return Ok(response);
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
  {
    var user = await _userManager.FindByEmailAsync(loginRequestDto.Email) ?? await _userManager.FindByNameAsync(loginRequestDto.Email);

    if (user == null) return Unauthorized("Invalid credentials");

    var passwordValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
    if (!passwordValid) return Unauthorized("Invalid credentials");

    var roles = await _userManager.GetRolesAsync(user);
    var (token, expires) = await _tokenRepository.CreateTokenAsync(user, roles);

    return Ok(new LoginResponseDto { Token = token, ExpiresAtUtc = expires });
  }




}