using TaskTrackerCat.HttpModels;

namespace TaskTrackerCat.Infrastructure;

public class JwtTokenHelper
{
    public TokenViewModel GetToken()
    {
        return new TokenViewModel();
    }
}