using CodeReview.TopicManagement.Api.TopicProposal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeReview.TopicManagement.Api
{
    public static class Config
    {
        public static IServiceCollection AddEventHandler<TEvent, TEventHandler>(
            this IServiceCollection services
        )
            where TEventHandler : class, IEventHandler<TEvent>
        {
            return services
                .AddTransient<TEventHandler>()
                .AddTransient<IEventHandler<TEvent>>(sp => sp.GetRequiredService<TEventHandler>());
        }
    }
}
