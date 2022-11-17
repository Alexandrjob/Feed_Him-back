namespace TaskTrackerCat.Infrastructure.Handlers.Interfaces;

public interface IRequestHandler<T>
{
    public Task Handle(T model);
}