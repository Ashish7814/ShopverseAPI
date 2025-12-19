using IdentityService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ResponseDto> RegisterAsync(RegisterDto model);
        Task<LoginResponseDto> LoginAsync(LoginDto model);
        Task<ResponseDto> ForgotPasswordAsync(ForgotPasswordDto model);
        Task<ResponseDto> ResetPasswordAsync(string userId, string encodedToken);
    }
}
