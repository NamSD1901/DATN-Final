using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using MyPetClinic.Application.DTOs.Notification;
using MyPetClinic.Application.Interfaces.Services;

namespace MyPetClinic.Infrastructure.Services
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailMessageDto> _queue;

        public EmailQueue()
        {
            // Unbounded channel: we don't drop emails, assuming the queue size is manageable.
            // For huge scale, a bounded channel with dropping strategies could be used.
            var options = new UnboundedChannelOptions
            {
                SingleWriter = false,
                SingleReader = true
            };
            _queue = Channel.CreateUnbounded<EmailMessageDto>(options);
        }

        public async ValueTask QueueEmailAsync(EmailMessageDto emailMessage)
        {
            ArgumentNullException.ThrowIfNull(emailMessage);
            await _queue.Writer.WriteAsync(emailMessage);
        }

        public async ValueTask<EmailMessageDto> DequeueEmailAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
