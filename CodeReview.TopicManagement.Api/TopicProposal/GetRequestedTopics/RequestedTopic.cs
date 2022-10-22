using Marten.Events.Aggregation;

namespace CodeReview.TopicManagement.Api.TopicProposal.GetRequestedTopics
{
    public record RequestedTopic(Guid Id, string Label, string Description, string Requester, 
        CodeReviewTopicStatus Status, DateTimeOffset? ScheduledAt = null);

    public class RequestedTopicHistoryTransformation : SingleStreamAggregation<RequestedTopic>
    {
        public static RequestedTopic Create(TopicRequested requested) =>
            new(requested.Id, requested.Label, requested.Description, requested.Requester, CodeReviewTopicStatus.Requested);

        public RequestedTopic Apply(TopicScheduled scheduled, RequestedTopic current) =>
            current with { 
                Status = CodeReviewTopicStatus.Scheduled,
                ScheduledAt = scheduled.ScheduleDate
            };
    }
}
