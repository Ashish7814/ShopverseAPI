using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Implementation
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task<JwtSecurityToken> GenerateJwtTokenAsync(ApplicationUser user)
        {

            //var roles = await _unitOfWork.userRepository.GetRolesAsync(user);

            //var claims = new List<Claim>
            //    {
            //        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            //        new Claim(JwtRegisteredClaimNames.Email, user.Email),
            //        new Claim("name", user.UserName ?? $"{user.FirstName} {user.LastName}"),
            //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            //    };

            //claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return Task.FromResult(token);
        }
    }
}
