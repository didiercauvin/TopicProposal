using CodeReview.TopicManagement.Api;
using CodeReview.TopicManagement.Api.TopicProposal;
using CodeReview.TopicManagement.Api.TopicProposal.GetRequestedTopics;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Threading;
using static CodeReview.TopicManagement.Api.StoreOptionsServices;
using static CodeReview.TopicManagement.Api.TopicProposal.TopicProposalService;
using static Microsoft.AspNetCore.Http.Results;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddMarten(sp => SetStoreOption(sp, builder.Configuration))
    .AddAsyncDaemon(DaemonMode.Solo);

builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddScoped<IEventBus, FakeEventBus>();
builder.Services.AddScoped<ICommandBus, FakeCommandBus>();
builder.Services.AddEventHandler<TopicRequested, TopicProposalSaga>();
builder.Services.AddCommandHandler<ProposeTopic, CommandHandlers>();
builder.Services.AddCommandHandler<ScheduleTopic, CommandHandlers>();

var app = builder.Build();

app.MapPost("api/requests/",
    async (
        ICommandBus commandBus,
        ProposeTopicRequest body,
        CancellationToken ct) =>
    {
        var topicId = Guid.NewGuid();

        await commandBus.Send(
            new ProposeTopic(topicId, body.Label, body.Description, body.Requester, body.ScheduleDate), 
            ct);

        return Created($"api/requests/{topicId}", topicId);
    }
).WithTags("TopicRequest");

app.MapPost("api/requests/{topicId:guid}/schedule",
    async (
        ICommandHandler<ScheduleTopic> commandHandler,
        Guid topicId,
        ScheduleTopicRequest body,
        CancellationToken ct) => await commandHandler.Handle(new ScheduleTopic(topicId), ct)
).WithTags("TopicRequest");

app.MapPost("api/requests/{topicId:guid}/propose-reschedule",
    async (
        IDocumentSession documentSession,
        Guid topicId,
        ProposeTopicRescheduleRequest body,
        CancellationToken ct) =>
    {
        await documentSession.GetAndUpdate<CodeReviewTopic>(topicId, topic =>
            Handle(topic, new ProposeRescheduleTopic(topicId, body.RescheduleDate)), ct);
    }
).WithTags("TopicRequest");

app.MapPost("api/requests/{topicId:guid}/accept-reschedule",
    async (
        IDocumentSession documentSession,
        Guid topicId,
        AcceptTopicRescheduleRequest body,
        CancellationToken ct) =>
    {
        await documentSession.GetAndUpdate<CodeReviewTopic>(topicId, topic =>
            Handle(topic, new AcceptTopicReschedule(topicId)), ct);
    }
).WithTags("TopicRequest");

app.MapPost("api/requests/{topicId:guid}/refuse-reschedule",
    async (
        IDocumentSession documentSession,
        Guid topicId,
        RefuseTopicRescheduleRequest body,
        CancellationToken ct) =>
    {
        await documentSession.GetAndUpdate<CodeReviewTopic>(topicId, topic =>
            Handle(topic, new RefuseTopicReschedule(topicId)), ct);
    }
).WithTags("TopicRequest");

app.MapGet("api/requests/",
    (IQuerySession querySession, [FromQuery] int? pageNumber, [FromQuery] int? pageSize,
            CancellationToken ct) =>
        querySession.Query<RequestedTopic>().ToPagedListAsync(pageNumber ?? 1, pageSize ?? 10, ct)
).WithTags("TopicRequest");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public record ProposeTopicRequest(string Label, string Description, DateTimeOffset ScheduleDate, string Requester);
public record ScheduleTopicRequest();
public record ProposeTopicRescheduleRequest(DateTimeOffset RescheduleDate);
public record AcceptTopicRescheduleRequest();
public record RefuseTopicRescheduleRequest();
