using Hangfire;
using POS.Shared.Infrastructure;
using System.Linq.Expressions;

namespace POS.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string Enqueue(Expression<Action> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    public string Enqueue<T>(Expression<Action<T>> methodCall)
    {
        return BackgroundJob.Enqueue<T>(methodCall);
    }

    public string Schedule(Expression<Action> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule<T>(methodCall, delay);
    }

    public void RecurringJob(string jobId, Expression<Action> methodCall, string cronExpression)
    {
        Hangfire.RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);
    }

    public void RecurringJob<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression)
    {
        Hangfire.RecurringJob.AddOrUpdate<T>(jobId, methodCall, cronExpression);
    }

    public bool Delete(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }
}