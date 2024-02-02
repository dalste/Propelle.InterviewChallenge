using Polly;
using Polly.Retry;
using Propelle.InterviewChallenge.Application.Domain;
using Propelle.InterviewChallenge.Application.Domain.Events;
using Propelle.InterviewChallenge.EventHandling;

namespace Propelle.InterviewChallenge.Application.EventHandlers
{
    public class SubmitDeposit : IEventHandler<DepositMade>
    {
        private readonly PaymentsContext _context;
        private readonly ISmartInvestClient _smartInvestClient;
        int  _maxRetries =30;

        public SubmitDeposit(PaymentsContext context, ISmartInvestClient smartInvestClient)
        {
            _context = context;
            _smartInvestClient = smartInvestClient;
        }

        public async Task Handle(DepositMade @event)
        {
            var deposit = await _context.Deposits.FindAsync(@event.Id);

            AsyncRetryPolicy retryPolicy = Policy
             .Handle<TransientException>()
            .WaitAndRetryAsync(30, retryAttempt =>
            TimeSpan.FromSeconds( retryAttempt) / 2);

            await
            retryPolicy.ExecuteAsync(
                async () => { await _smartInvestClient.SubmitDeposit(deposit.UserId, deposit.Amount); }) ;


        }
    }
}
