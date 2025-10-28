namespace LibrarySystem.Domain.Queries;

public interface IQueryHandler<TQuery, TResult> where TQuery : BaseQuery
{
    Task<QueryResult<TResult>> HandleAsync(TQuery query);
}
