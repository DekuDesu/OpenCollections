using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace OpenCollections
{
    public static class Extensions
    {

        /// <summary>
        /// Hooks the ResultCollection of Output to the Collection of Input
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="Output"></param>
        /// <param name="Input"></param>
        public static void OutputTo<TResult>(this IConcurrentOutput<TResult> Output, IConcurrentInput<TResult> Input)
        {
            Output.ResultCollection = Input.Collection;
        }

        /// <summary>
        /// Hooks the Collection of <paramref name="Input"/> to the ResultCollection of <paramref name="Output"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Input"></param>
        /// <param name="Output"></param>
        public static void InputFrom<T>(this IConcurrentInput<T> Input, IConcurrentOutput<T> Output)
        {
            Input.Collection = Output.ResultCollection;
        }

        /// <summary>
        /// Adds the ResultCollection of the <paramref name="Output"/> object to the Collection of the <paramref name="Input"/> multi-consumer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Input"></param>
        /// <param name="Output"></param>
        public static void InputFrom<T, U>(this IConcurrentMultiConsumer<T, U> Input, IConcurrentOutput<T> Output)
        {
            Input.Collections.Add(Output.ResultCollection);
        }

        public static void StopObserving<T>(this IConcurrentInput<T> ObjectThatConsumesItems, IConcurrentOutput<T> ObjectThatProducesItems)
        {
            ObjectThatProducesItems.UnHookEvents(ObjectThatConsumesItems);
        }

        /// <summary>
        /// Subcsribes <paramref name="ObjectThatConsumesItems"/> to <paramref name="ObjectThatProducesItems"/>, so that when <paramref name="ObjectThatProducesItems"/> produces items <paramref name="ObjectThatConsumesItems"/> begins consuming
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ObjectThatConsumesItems"></param>
        /// <param name="ObjectThatProducesItems"></param>
        public static void ObserveCollection<T>(this IConcurrentInput<T> ObjectThatConsumesItems, IConcurrentOutput<T> ObjectThatProducesItems)
        {
            ObjectThatProducesItems.HookEvents(ObjectThatConsumesItems);
        }

        public static void StopObservingAsync<T>(this IConcurrentInput<T> ObjectThatConsumesItems, IConcurrentOutput<T> ObjectThatProducesItems)
        {
            ObjectThatProducesItems.UnHookEventsAsync(ObjectThatConsumesItems);
        }

        /// <summary>
        /// Subcsribes <paramref name="ObjectThatConsumesItems"/> to <paramref name="ObjectThatProducesItems"/>, so that when <paramref name="ObjectThatProducesItems"/> produces items <paramref name="ObjectThatConsumesItems"/> begins consuming asynchronously
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ObjectThatConsumesItems"></param>
        /// <param name="ObjectThatProducesItems"></param>
        public static void ObserveCollectionAsync<T>(this IConcurrentInput<T> ObjectThatConsumesItems, IConcurrentOutput<T> ObjectThatProducesItems)
        {
            ObjectThatProducesItems.HookEventsAsync(ObjectThatConsumesItems);
        }

        private static void HookEvents(this IConcurrentEvent host, IConcurrentEvent subscriber)
        {
            host.Finished += subscriber.Invoke;
            host.CollectionChanged += subscriber.Invoke;
            host.Started += subscriber.Invoke;
        }

        private static void UnHookEvents(this IConcurrentEvent host, IConcurrentEvent subscriber)
        {
            host.Finished -= subscriber.Invoke;
            host.CollectionChanged -= subscriber.Invoke;
            host.Started -= subscriber.Invoke;
        }

        private static void HookEventsAsync(this IConcurrentEvent host, IConcurrentEvent subscriber)
        {
            host.CollectionChangedAsync += subscriber.InvokeAsync;
        }

        private static void UnHookEventsAsync(this IConcurrentEvent host, IConcurrentEvent subscriber)
        {
            host.CollectionChangedAsync -= subscriber.InvokeAsync;
        }
    }
}
