﻿namespace MassTransit.Context
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;
    using Metadata;
    using Util;


    public class RetryConsumeContext :
        ConsumeContextScope,
        ConsumeRetryContext
    {
        readonly ConsumeContext _context;
        readonly PendingFaultCollection _pendingFaults;

        public RetryConsumeContext(ConsumeContext context, IRetryPolicy retryPolicy, RetryContext retryContext)
            : base(context)
        {
            RetryPolicy = retryPolicy;
            _context = context;

            if (retryContext != null)
            {
                RetryAttempt = retryContext.RetryAttempt;
                RetryCount = retryContext.RetryCount;
            }

            _pendingFaults = new PendingFaultCollection();
        }

        protected IRetryPolicy RetryPolicy { get; }

        public int RetryAttempt { get; }

        public int RetryCount { get; }

        public virtual TContext CreateNext<TContext>(RetryContext retryContext)
            where TContext : class, ConsumeRetryContext
        {
            throw new InvalidOperationException("This is only supported by a derived type");
        }

        public Task NotifyPendingFaults()
        {
            return _pendingFaults.Notify(_context);
        }

        public override Task NotifyFaulted<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception)
        {
            if (RetryPolicy.IsHandled(exception))
            {
                _pendingFaults.Add(new PendingFault<T>(context, duration, consumerType, exception));
                return TaskUtil.Completed;
            }

            return _context.NotifyFaulted(context, duration, consumerType, exception);
        }

        public RetryConsumeContext CreateNext(RetryContext retryContext)
        {
            return new RetryConsumeContext(_context, RetryPolicy, retryContext);
        }
    }


    public class RetryConsumeContext<T> :
        RetryConsumeContext,
        ConsumeContext<T>
        where T : class
    {
        readonly ConsumeContext<T> _context;

        public RetryConsumeContext(ConsumeContext<T> context, IRetryPolicy retryPolicy, RetryContext retryContext)
            : base(context, retryPolicy, retryContext)
        {
            _context = context;
        }

        T ConsumeContext<T>.Message => _context.Message;

        public Task NotifyConsumed(TimeSpan duration, string consumerType)
        {
            return NotifyConsumed(_context, duration, consumerType);
        }

        public Task NotifyFaulted(TimeSpan duration, string consumerType, Exception exception)
        {
            return NotifyFaulted(_context, duration, consumerType, exception);
        }

        public override TContext CreateNext<TContext>(RetryContext retryContext)
        {
            return new RetryConsumeContext<T>(_context, RetryPolicy, retryContext) as TContext
                ?? throw new ArgumentException($"The context type is not valid: {TypeMetadataCache<T>.ShortName}");
        }
    }
}
