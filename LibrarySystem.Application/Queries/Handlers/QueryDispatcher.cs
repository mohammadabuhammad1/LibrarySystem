using LibrarySystem.Domain.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.Application.Queries.Handlers;

public class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public async Task<QueryResult<TResult>> DispatchAsync<TQuery, TResult>(TQuery query)
        where TQuery : BaseQuery
    {
        IQueryHandler<TQuery, TResult>? handler = serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();
        return handler == null
            ? throw new InvalidOperationException($"No query handler registered for {typeof(TQuery).Name}")
            : await handler.HandleAsync(query).ConfigureAwait(false);
    }
}

public interface IQueryDispatcher
{
    Task<QueryResult<TResult>> DispatchAsync<TQuery, TResult>(TQuery query)
        where TQuery : BaseQuery;
}
