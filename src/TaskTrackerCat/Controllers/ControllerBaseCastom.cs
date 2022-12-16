using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerCat.Repositories.Interfaces;
using TaskTrackerCat.Repositories.Models;

namespace TaskTrackerCat.Controllers;

public class ControllerBaseCastom : ControllerBase
{
    protected readonly IUserRepository _userRepository;

    protected ControllerBaseCastom()
    {
    }

    public ControllerBaseCastom(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Возвращает текущего пользователя, читая email из токена. 
    /// </summary>
    /// <returns></returns>
    protected async Task<UserDto> GetUserAsync()
    {
        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var tokenUserEmail = jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value;

        var currentUser = new UserDto
        {
            Email = tokenUserEmail
        };

        var user = await _userRepository.GetUserAsync(currentUser);
        return user;
    }
}