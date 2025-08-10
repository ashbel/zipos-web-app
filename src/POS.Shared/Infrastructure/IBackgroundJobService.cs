using System.Linq.Expressions;

namespace POS.Shared.Infrastructure;

public interface IBackgroundJobService
{
    string Enqueue(Expression<Action> methodCall);
    string Enqueue<T>(Expression<Action<T>> methodCall);
    string Schedule(Expression<Action> methodCall, TimeSpan delay);
    string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
    void RecurringJob(string jobId, Expression<Action> methodCall, string cronExpression);
    void RecurringJob<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression);
    bool Delete(string jobId);
}