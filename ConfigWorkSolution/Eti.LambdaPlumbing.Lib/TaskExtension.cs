using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eti.LambdaPlumbing
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Allows you to set a timeout for an async Task. If the task does not complete
        /// before the timeout, a TimeoutException is thrown. Also note that it will attempt
        /// to cancel the running task.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="milliseconds"></param>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int milliseconds) {

            using (var timeoutCancellationTokenSource = new CancellationTokenSource()) {

                var completedTask = await Task.WhenAny(task, Task.Delay(milliseconds, timeoutCancellationTokenSource.Token));
                if (completedTask == task) {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  
                } else {
                    throw new TimeoutException();
                }
            }
        }
    }
}