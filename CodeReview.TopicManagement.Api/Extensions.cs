using Marten;

public static class Extensions
{
    public static async Task Save<T>(this IDocumentSession documentSession, Guid id, object @event, CancellationToken ct)
        where T : class
    {
        documentSession.Events.StartStream<T>(id, @event);
        await documentSession.SaveChangesAsync(token: ct);
    }

    public static Task GetAndUpdate<T>(this IDocumentSession documentSession, Guid id,
        Func<T, object> handle, CancellationToken ct)
        where T : class =>
        documentSession.Events.WriteToAggregate<T>(id, stream =>
            stream.AppendOne(handle(stream.Aggregate)), ct);
}
