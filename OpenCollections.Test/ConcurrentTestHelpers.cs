﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCollections.Tests
{
    /// <summary>
    /// This is a version of ConcurrentQueue that allows to disable either adding or removing items, perfect for testing failures or delays in asynchronous methods
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BrokenConcurrentQueue<T> : IProducerConsumerCollection<T>
    {
        private List<T> Data = new List<T>();

        public bool AllowAdding { get; set; } = false;
        public bool AllowRemoving { get; set; } = false;

        public int Count => Data.Count;
        public object SyncRoot { get; }
        public bool IsSynchronized { get; } = true;

        public void CopyTo(T[] array, int index)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();

        public T[] ToArray() => Data.ToArray();

        public bool TryAdd(T item)
        {
            if (AllowAdding)
            {
                Data.Add(item);
                return true;
            }
            return false;
        }

        public bool TryTake(out T item)
        {
            if (AllowRemoving)
            {
                item = Data.First();
                Data.RemoveAt(0);
                return true;
            }

            item = default;

            return false;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Data.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Data.GetEnumerator();
    }
    public static class ConcurrentTestHelpers
    {
        public static void Add<T>(this ConcurrentQueue<T> queue, T item)
        {
            queue.Enqueue(item);
        }

        public static (bool result, string message) VerifyCollection<T>(IEnumerable<T> expected, IEnumerable<T> actual) where T : IEquatable<T>
        {
            if (expected is null ^ actual is null)
            {
                string expectedNull = expected is null ? "null" : "not null";
                string actualNull = actual is null ? "null" : "not null";
                return (false, $"Expected: {expectedNull} Actual: {actualNull}");
            }
            if (expected.Count() != actual.Count())
            {
                return (false, $"Collection sizes different: Expected: {expected.Count()} Actual: {actual.Count()}");
            }

            T[] expectedArray = expected.ToArray();
            T[] actualArray = actual.ToArray();

            for (int i = 0; i < expected.Count(); i++)
            {
                if (expectedArray[i].Equals(actualArray[i]) == false)
                {
                    return (false, $"({i})Expected: {expectedArray[i]} Actual: {actualArray[i]}");
                }
            }

            return (true, "");
        }

        public static (bool result, string message) VerifyAllValuesPresent<T>(IEnumerable<T> expectedValues, IEnumerable<T> actualValues)
        {
            (bool result, string message) value = (true, null);
            foreach (var item in expectedValues)
            {
                if (actualValues.Contains(item) == false)
                {
                    value.result = false;
                    value.message += $"{Environment.NewLine} Expected: {item} Actual: Not Found";
                }
            }
            return value;
        }
        public static (bool result, string message) VerifyAllValuesPresent<T>(IEnumerable<T> expectedValues, IEnumerable<IEnumerable<T>> collectionOfActualValues)
        {
            foreach (var item in collectionOfActualValues)
            {
                var result = VerifyAllValuesPresent(expectedValues, item);
                if (result.result == false)
                {
                    return result;
                }
            }
            return (result: true, message: null);
        }
    }
}
