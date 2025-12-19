using IdentityService.Application.DTOs;
using IdentityService.Application.Helper;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;
        public AuthService(IAuthRepository authRepository, IConfiguration configuration, ITokenService tokenService, IEmailSender emailSender)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _tokenService = tokenService;
            _emailSender = emailSender;
        }
        public async Task<ResponseDto> RegisterAsync(RegisterDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                    return null;

                var normalizedEmail = model.Email.Trim().ToLowerInvariant();
                var existing = await _authRepository.GetFirstOrDefaultAsync(normalizedEmail);
                if (existing != null)
                {
                    return new ResponseDto { Status = "Error", Message = "User already exists!" };
                }
                // 2. Build user entity
                var user = new ApplicationUser
                {
                    Email = normalizedEmail,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                };
                var result = await _authRepository.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    return new ResponseDto
                    {
                        Status = "Error",
                        Message = $"User creation failed! {errors}",
                        Succeeded = false,
                        UserId = user.Id
                    };
                }
                var token = await _authRepository.GenerateEmailToken(user);
                var tokenBytes = Encoding.UTF8.GetBytes(token);
                var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);

                var baseUrl = _configuration["App:BaseUrl"]?.TrimEnd('/');
                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Fallback to building relative path (not recommended for production)
                    baseUrl = _configuration["ASPNETCORE_HOSTING_URL"] ?? throw new InvalidOperationException("App:BaseUrl is not configured.");
                }
                // Endpoint that will call the API to confirm email (this service exposes ConfirmEmailAsync)
                var callbackUrl = $"{baseUrl}/Auth/confirm-email?userId={user.Id}&token={encodedToken}";

                // Compose HTML email
                var subject = "Confirm your ShiftEase account";
                var htmlBody = HtmlTemplate.BuildEmailHtml(user.FirstName ?? user.UserName ?? "User", callbackUrl);

                // Send email (sync-over-async avoided; await)
                await _emailSender.SendEmailAsync(user.Email, subject, htmlBody);

                return new ResponseDto
                {
                    Status = "Success",
                    Message = "User created successfully!",
                    Succeeded = true,
                    UserId = user.Id,
                    EncodedToken = encodedToken
                };
            }
            catch (Exception ex)

            {
                return new ResponseDto
                {
                    Status = "Error",
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                    return null;

                ApplicationUser user = null;
                var normalizedEmail = model.Username.Trim().ToLowerInvariant();
                if (normalizedEmail.Contains("@"))
                {
                    user = await _authRepository.GetFirstOrDefaultAsync(normalizedEmail);
                }
                else
                {
                    user = await _authRepository.GetFirstOrDefaultAsync(normalizedEmail);
                }
                if (user == null)
                    return new LoginResponseDto { Status = "Error", Message = "Invalid User.", Succeeded = false };

                var userRele = await _authRepository.GetUserRolesAsync(user.Id);
                if (userRele == null)
                    return new LoginResponseDto { Status = "Error", Message = "Invalid UserRole.", Succeeded = false };

                var passwordValid = await _authRepository.CheckPasswordAsync(user, model.Password);
                if (!passwordValid)
                    return new LoginResponseDto { Status = "Error", Message = "Invalid credentials.", Succeeded = false };

                var confirmEmail = await _authRepository.IsEmailConfirmedAsync(user);
                if (!confirmEmail)
                    return new LoginResponseDto { Status = "Error", Message = "Email not confirmed.", Succeeded = false };

                var token = await _tokenService.GenerateJwtTokenAsync(user);
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return new LoginResponseDto
                {
                    Status = "Success",
                    Message = "Login successful.",
                    Token = tokenString,
                    Expiration = token.ValidTo,
                    Role = userRele,
                    Succeeded = true,
                    UserId = user.Id
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDto
                {
                    Status = "Error",
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        public async Task<ResponseDto> ForgotPasswordAsync(ForgotPasswordDto model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                    return null;

                ApplicationUser user = null;
                var normalizedEmail = model.Email.Trim().ToLowerInvariant();
                var existing = await _authRepository.GetFirstOrDefaultAsync(normalizedEmail);
                if (existing == null)
                {
                    return new ResponseDto { Status = "Error", Message = "Email not exist", Succeeded = false };
                }
                var token = await _authRepository.GeneratePasswordResetToken(existing);
                var tokenByte = Encoding.UTF8.GetBytes(token);
                var encodedToken = WebEncoders.Base64UrlEncode(tokenByte);

                var baseUrl = _configuration["App:BaseUrl"]?.TrimEnd('/');
                if (string.IsNullOrEmpty(baseUrl))
                {
                    // Fallback to building relative path (not recommended for production)
                    baseUrl = _configuration["ASPNETCORE_HOSTING_URL"] ?? throw new InvalidOperationException("App:BaseUrl is not configured.");
                }
                // Endpoint that will call the API to confirm email (this service exposes ConfirmEmailAsync)
                var callbackUrl = $"{baseUrl}/Auth/reset-password?userId={existing.Id}&token={encodedToken}";
                var subject = "Reset your ShiftEase password";
                var htmlBody = HtmlTemplate.BuildForgotPasswordEmailHtml(existing.FirstName ?? existing.UserName ?? "User", callbackUrl);
                await _emailSender.SendEmailAsync(existing.Email, subject, htmlBody);

                return new ResponseDto
                {
                    Status = "Success",
                    Message = "Forgot Email successfully!",
                    Succeeded = true,
                    UserId = existing.Id,
                    EncodedToken = encodedToken
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    Status = "Error",
                    Message = ex.Message,
                    Succeeded = false
                };
            }
        }

        public Task<ResponseDto> ResetPasswordAsync(string userId, string encodedToken)
        {
            throw new NotImplementedException();
        }

        //public async Task<ResponseDto> ResetPasswordAsync(string userId, string encodedToken)
        //{
        //    try
        //    {
        //        throw new ExceptionHandlerExtensions();
        //        //if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(encodedToken))
        //        //    return null;
        //        //var user = await _unitOfWork.userRepository.GetByIdAsync(userId);
        //        //if (user == null)
        //        //{
        //        //    return new ResponseDto { Status = "Error", Message = "Invalid user", Succeeded = false };
        //        //}
        //        //var tokenBytes = WebEncoders.Base64UrlDecode(encodedToken);
        //        //var token = Encoding.UTF8.GetString(tokenBytes);
        //        //var result = await _unitOfWork.userRepository.ResetPasswordAsync(user, token);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseDto
        //        {
        //            Status = "Success",
        //            Message = "Forgot Email successfully!",
        //            Succeeded = true,
        //            //UserId = existing.Id,
        //            EncodedToken = encodedToken
        //        };
        //    }
        //}
    }
}
