﻿using CodeReview.TopicManagement.Api.TopicProposal;
using Marten;

namespace CodeReview.TopicManagement.Api.TopicProposal
{
    public record ProposeTopic(Guid TopicId, string Label, string Description, string Requester);
    public record ScheduleTopic(Guid IdTopic, DateTimeOffset ScheduleDate);

    public static class TopicProposalService
    {
        public static TopicRequested Handle(ProposeTopic command)
        {
            return new TopicRequested(command.TopicId, command.Label, command.Description, command.Requester);
        }

        public static TopicScheduled Handle(CodeReviewTopic topic, ScheduleTopic command)
        {
            return new TopicScheduled(command.IdTopic, command.ScheduleDate);
        }
    }
}
