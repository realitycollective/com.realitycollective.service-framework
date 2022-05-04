using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RealityToolkit.ServiceFramework.Utilities.Async.AwaitYieldInstructions
{
    /// <summary>
    /// Helper class for continuing executions on a background thread.
    /// </summary>
    public class BackgroundThread
    {
        public static ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            return Task.Run(() => { }).ConfigureAwait(false).GetAwaiter();
        }
    }
}