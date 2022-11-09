namespace TaskTrackerCat.Infrastructure.Handlers.Interfaces;

public interface IRequestHandler<in T>
{
    public Task Handle(T type);
}