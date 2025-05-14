using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BookStoreAPI.Dtos;
using BookStoreAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using BookStoreAPI.Helpers;

namespace BookStoreAPI.Controllers;
[Authorize]
[Route("/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly MailHelper _mailHelper;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config, MailHelper mailHelper,ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _mailHelper = mailHelper;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid){
            _logger.LogWarning("Registration attempt with invalid model state for email: {Email}", model.Email);
            return BadRequest(ModelState);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userManager.CreateAsync(user, model.Password);


        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} registered successfully at time {time}", model.Email, DateTime.UtcNow);
            // Assign the "User" role to every new registered user
            await _userManager.AddToRoleAsync(user, "User");

            // Send a confirmation email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/auth/confirm-email?email={user.Email}&token={encodedToken}";
            await _mailHelper.SendEmailAsync(user.Email, "Email Confirmation", $"Please confirm your email by clicking this link: {confirmationLink}");

            // Log the registration
            return Ok(new { message = "User registered successfully" });
        }

        return BadRequest(result.Errors);

    }
    
    // email confirmation endpoint
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return BadRequest(new { message = "Invalid email " });
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var decodedToken = System.Web.HttpUtility.UrlDecode(token);
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (result.Succeeded)
        {
            return Ok(new { message = "Email confirmed successfully" });
        }

        return BadRequest(result.Errors);
    }




    // login endpoint
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null){
            _logger.LogWarning("Login attempt with invalid email: {Email}",model.Email);
            return Unauthorized(new { message = "Invalid credentials" });
        }
        if (!await _userManager.IsEmailConfirmedAsync(user)){
            _logger.LogWarning("Login attempt with unconfirmed email: {Email}",model.Email);
            return Unauthorized(new { message = "Email not confirmed" });
        }
        

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded){
            _logger.LogWarning("Login attempt with invalid password for email: {Email}",model.Email);
            // Log the failed login attempt
            return Unauthorized(new { message = "Invalid credentials" });
            
        }
        _logger.LogInformation("User {Email} logged in successfully", model.Email);
        // Generate JWT token

        var token = GenerateJwtToken(user);
        return Ok(new { token });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {

        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("FullName", user.FullName)
            };
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // adding an endpoint where admin can assign roles to users
    [HttpPost("assign-role")]
    [Authorize(Roles = "Admin")] // Only admins can assign roles
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null){
            _logger.LogWarning("Attempt to assign role to non-existing user: {Email}", model.Email);
            return NotFound(new { message = "User not found" });
        }

        var result = await _userManager.AddToRoleAsync(user, model.Role);
        if (result.Succeeded){
            _logger.LogInformation("Role {Role} assigned to user {Email}", model.Role, model.Email);
            // Log the role assignment
            return Ok(new { message = $"Role '{model.Role}' assigned to {user.Email}" });
        }

        return BadRequest(result.Errors);
    }

    // request to reset password endpoint
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            _logger.LogWarning("Attempt to reset password for non-existing user: {Email}", model.Email);
            return Ok(new { message = " A reset link has been sent to your email" });

        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = System.Web.HttpUtility.UrlEncode(token);

        var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?email={model.Email}&token={encodedToken}";

        //send the email via mailtrap
        await _mailHelper.SendEmailAsync(model.Email, "Password Reset", $"Reset your password using this link: {resetLink}");

        return Ok(new { message = " A reset link has been sent to your email" });

    }

    // reset password endpoint
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null){
            _logger.LogWarning("Attempt to reset password for non-existing user: {Email}", model.Email);
            return BadRequest(new { message = "Invalid request" });
        }

        var decodedToken = System.Web.HttpUtility.UrlDecode(model.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

        if (result.Succeeded){
            _logger.LogInformation("Password reset successfully for user {Email} at time :{time}", model.Email, DateTime.UtcNow);
            return Ok(new { message = "Password has been reset successfully" });
        }

        return BadRequest(result.Errors);
    }

}
