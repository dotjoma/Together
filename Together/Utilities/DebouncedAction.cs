using System;
using System.Threading;
using System.Threading.Tasks;

namespace Together.Presentation.Utilities
{
    /// <summary>
    /// Utility class for debouncing actions (e.g., search inputs)
    /// </summary>
    public class DebouncedAction
    {
        private readonly int _delayMilliseconds;
        private CancellationTokenSource? _cancellationTokenSource;

        public DebouncedAction(int delayMilliseconds = 300)
        {
            _delayMilliseconds = delayMilliseconds;
        }

        /// <summary>
        /// Debounces an action - only executes after the specified delay with no new calls
        /// </summary>
        public void Debounce(Action action)
        {
            // Cancel previous pending action
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var token = _cancellationTokenSource.Token;

            Task.Delay(_delayMilliseconds, token).ContinueWith(task =>
            {
                if (!task.IsCanceled)
                {
                    action();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Debounces an async action - only executes after the specified delay with no new calls
        /// </summary>
        public void Debounce(Func<Task> asyncAction)
        {
            // Cancel previous pending action
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var token = _cancellationTokenSource.Token;

            Task.Delay(_delayMilliseconds, token).ContinueWith(async task =>
            {
                if (!task.IsCanceled)
                {
                    await asyncAction();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Cancels any pending debounced action
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
