using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskTrackerCat.Infrastructure.Identity;

public class AuthOptions
{
    public const string ISSUER = "FeedHim";
    public const string AUDIENCE = "FeedHim.com";
    public const string KEY = "0x7e57b0920210d400000";

    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}