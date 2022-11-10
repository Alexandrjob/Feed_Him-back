using TaskTrackerCat.Controllers;
using TaskTrackerCat.HttpModels;

namespace TaskTrackerCat.Infrastructure.Handlers.Interfaces;

public interface IRequestAuthenticationHandler<T, R>
{
    public Task<AuthenticationUserViewModel> Handle(T type);
}