using AutoMapper;
using System;

namespace RealWorld.Infrastructure
{
    /// <summary>
    /// In general usage, AutoMapper will only be initialized once. However,
    /// while running unit tests though xUnit, tests are ran concurrently.
    /// This behavior led to AutoMapper being initialized multiple times. 
    /// </summary>
    public static class AutoMapperConfiguration
    {
        private static readonly object _lock = new object();
        private static bool _initialized;
        public static void Initialize(Action<IMapperConfigurationExpression> config)
        {
            lock (_lock)
            {
                if (_initialized) return;

                Mapper.Initialize(config);
                _initialized = true;
            }
        }
    }
}
