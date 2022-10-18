using CodeReview.TopicManagement.Api;
using CodeReview.TopicManagement.Api.TopicProposal;
using CodeReview.TopicManagement.Api.TopicProposal.GetRequestedTopics;
using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
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
builder.Services.AddEventHandler<TopicRequested, TopicProposalSaga>();

var app = builder.Build();

app.MapPost("api/requests/",
    async (
        IDocumentSession documentSession,
        ProposeTopicRequest body,
        CancellationToken ct) =>
    {
        var topicId = Guid.NewGuid();

        documentSession.Events.StartStream<CodeReviewTopic>(topicId, Handle(new ProposeTopic(topicId, body.Label, body.Description, body.Requester)));
        await documentSession.SaveChangesAsync(token: ct);

        return Created($"api/requests/{topicId}", topicId);
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

public record ProposeTopicRequest(string Label, string Description, string Requester);

