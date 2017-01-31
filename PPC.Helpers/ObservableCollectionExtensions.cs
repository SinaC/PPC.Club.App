using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PPC.Helpers
{
    public static class ObservableCollectionExtensions
    {
        public static int RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
        {
            var itemsToRemove = collection.Where(condition).ToArray();

            foreach (var itemToRemove in itemsToRemove)
                collection.Remove(itemToRemove);

            return itemsToRemove.Length;
        }

        public static int RemoveOfType<T,U>(this ObservableCollection<T> collection)
            where U : T
        {
            var itemsToRemove = collection.OfType<U>().ToArray();

            foreach (var itemToRemove in itemsToRemove)
                collection.Remove(itemToRemove);

            return itemsToRemove.Length;
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach(T item in items)
                collection.Add(item);
        }
    }
}
