using Marten;
using System;
using static CodeReview.TopicManagement.Api.TopicProposal.TopicProposalService;

namespace CodeReview.TopicManagement.Api.TopicProposal
{

    public class TopicProposalSaga : IEventHandler<TopicRequested>
    {
        private readonly IDocumentSession _documentSession;

        public TopicProposalSaga(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        public Task Handle(TopicRequested requested, CancellationToken cancellationToken)
        {
            //_documentSession.Events.WriteToAggregate<CodeReviewTopic>(requested.Id, stream =>
            //stream.AppendOne(TopicProposalService.Handle(new ScheduleTopic(requested.Id))), cancellationToken);

            return Task.CompletedTask;
        }
    }

}
