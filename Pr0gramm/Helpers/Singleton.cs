﻿using System;
using System.Collections.Concurrent;

namespace Pr0gramm.Helpers
{
    internal static class Singleton<T>
        where T : new()
    {
        private static readonly ConcurrentDictionary<Type, T> _instances = new ConcurrentDictionary<Type, T>();

        public static T Instance
        {
            get { return _instances.GetOrAdd(typeof(T), t => new T()); }
        }
    }
}
