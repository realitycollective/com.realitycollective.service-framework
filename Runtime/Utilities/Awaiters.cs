// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using RealityToolkit.ServiceFramework.Utilities.AwaitYieldInstructions;

namespace RealityToolkit.ServiceFramework.Utilities
{
    public static class Awaiters
    {
        /// <summary>
        /// Use this awaiter to continue execution on the main thread.
        /// </summary>
        /// <remarks>Brings the execution back to the main thead on the next engine update.</remarks>
        public static UnityMainThread UnityMainThread { get; } = new UnityMainThread();

        /// <summary>
        /// Use this awaiter to continue execution on the background thread.
        /// </summary>
        public static BackgroundThread BackgroundThread { get; } = new BackgroundThread();

        /// <summary>
        /// Use this awaiter to wait until the condition is met.<para/>
        /// Author: Oguzhan Soykan<para/>
        /// Source: https://stackoverflow.com/questions/29089417/c-sharp-wait-until-condition-is-true
        /// </summary>
        /// <remarks>Passing in -1 will make this wait indefinitely for the condition to be met.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <param name="predicate">The predicate condition to meet.</param>
        /// <param name="timeout">The number of seconds before timing out and throwing an exception. (-1 is indefinite)</param>
        /// ReSharper disable once ExceptionNotThrown
        /// <exception cref="TimeoutException">A <see cref="TimeoutException"/> can be thrown when the condition isn't satisfied after timeout.</exception>
        public static async Task<T> WaitUntil<T>(this T element, Func<T, bool> predicate, int timeout = 10)
        {
            if (timeout == -1)
            {
                return await WaitUntil_Indefinite(element, predicate);
            }

            using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout)))
            {
                var tcs = new TaskCompletionSource<object>();

                void Exception()
                {
                    tcs.TrySetException(new TimeoutException());
                    tcs.TrySetCanceled();
                }

                cancellationTokenSource.Token.Register(Exception);
#if UNITY_EDITOR
                var editorCancelled = false;
                UnityEditor.EditorApplication.playModeStateChanged += playModeStateChanged => editorCancelled = true;
#endif

                while (!cancellationTokenSource.IsCancellationRequested)
                {
#if UNITY_EDITOR
                    if (editorCancelled)
                    {
                        tcs.TrySetCanceled(CancellationToken.None);
                    }
#endif
                    try
                    {
                        if (!predicate(element))
                        {
                            await Task.Delay(1, cancellationTokenSource.Token);
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        tcs.TrySetException(e);
                    }

                    tcs.TrySetResult(Task.CompletedTask);
                    break;
                }

                await tcs.Task;
            }

            return element;
        }

        private static async Task<T> WaitUntil_Indefinite<T>(T element, Func<T, bool> predicate)
        {
            var tcs = new TaskCompletionSource<object>();

#if UNITY_EDITOR
            var editorCancelled = false;
            UnityEditor.EditorApplication.playModeStateChanged += playModeStateChanged => editorCancelled = true;
#endif
            while (true)
            {
#if UNITY_EDITOR
                if (editorCancelled)
                {
                    tcs.TrySetCanceled(CancellationToken.None);
                }
#endif
                try
                {
                    if (!predicate(element))
                    {
                        await Task.Delay(1);
                        continue;
                    }
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }

                tcs.TrySetResult(Task.CompletedTask);
                break;
            }

            await tcs.Task;
            return element;
        }
    }
}