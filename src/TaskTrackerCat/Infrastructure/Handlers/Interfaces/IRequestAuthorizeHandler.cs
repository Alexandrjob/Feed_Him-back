namespace TaskTrackerCat.Infrastructure.Handlers.Interfaces;

public interface IRequestAuthorizeHandler<T, R>
{
    public Task<R> Handle(T type);
}