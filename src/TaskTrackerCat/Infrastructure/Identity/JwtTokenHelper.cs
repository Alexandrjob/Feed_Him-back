using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using TaskTrackerCat.HttpModels;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Infrastructure.Identity;

public class JwtTokenHelper
{
    public TokenViewModel GetToken(UserDto user)
    {
        var claims = new List<Claim>
            {new Claim(ClaimTypes.Email, user.Email), new Claim(ClaimTypes.GroupSid, user.GroupId.ToString())};
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)), // время действия 1 день
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                SecurityAlgorithms.HmacSha256));

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new TokenViewModel()
        {
            AccessToken = encodedJwt
        };
    }
}