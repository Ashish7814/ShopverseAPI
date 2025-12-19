using IdentityService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Interfaces
{
    public interface ITokenService
    {
        Task<JwtSecurityToken> GenerateJwtTokenAsync(ApplicationUser user);
    }
}
