using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Domain.Interfaces
{
    public interface IAuthRepository
    {
        Task<ApplicationUser?> GetFirstOrDefaultAsync(string email);

        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);

        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);

        Task<bool> IsEmailConfirmedAsync(ApplicationUser user);

        Task<string> GenerateEmailToken(ApplicationUser user);

        Task<string> GeneratePasswordResetToken(ApplicationUser user);

        Task<ApplicationUser> GetByIdAsync(string id);

        Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
        Task<string> GetUserRolesAsync(string userId);
    }
}
