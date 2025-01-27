﻿namespace TaskTrackerCat.Infrastructure.Handlers.Interfaces;

public interface IRequestAuthenticationHandler<T, R>
{
    public Task<R> Handle(T type);
}