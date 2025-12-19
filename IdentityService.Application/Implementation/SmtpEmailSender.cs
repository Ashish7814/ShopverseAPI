using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Implementation
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _stmpOtptions;
        private readonly IAuthRepository _authRepository;

        public SmtpEmailSender(IOptions<SmtpOptions> smtpOptions, IAuthRepository authRepository)
        {
            _stmpOtptions = smtpOptions.Value;
            _authRepository = authRepository;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var msg = new MailMessage
                {
                    From = new MailAddress(_stmpOtptions.From, _stmpOtptions.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                msg.To.Add(new MailAddress(toEmail));
                using var client = new SmtpClient(_stmpOtptions.Host, _stmpOtptions.Port)
                {
                    EnableSsl = _stmpOtptions.EnableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(_stmpOtptions.UserName, _stmpOtptions.Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };
                client.Timeout = 10000;
                await client.SendMailAsync(msg);
            }
            catch (SmtpException smtpEx)
            {
                throw;
            }
        }

        public async Task<ResponseDto> ConfirmEmailAsync(string userId, string encodedToken)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(encodedToken))
                    return new ResponseDto { Status = "Error", Message = "Invalid confirmation data." };

                var user = await _authRepository.GetByIdAsync(userId);
                if (user == null)
                    return new ResponseDto { Status = "Error", Message = "User not found." };

                var tokenBytes = WebEncoders.Base64UrlDecode(encodedToken);
                var token = Encoding.UTF8.GetString(tokenBytes);

                var result = await _authRepository.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return new ResponseDto
                    {
                        Status = "Success",
                        Message = "Email confirmed successfully.",
                        Succeeded = true,
                        UserId = user.Id
                    };
                }
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return new ResponseDto
                {
                    Status = "Error",
                    Message = $"Email confirmation failed: {errors}",
                    Succeeded = false,
                    UserId = user.Id
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
    }
}
