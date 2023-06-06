using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using R8.XunitLogger.Options;

namespace R8.XunitLogger
{
    public static class XunitForwardingLoggerExtensions
    {
        /// <summary>
        /// Adds an xunit logger named 'Xunit' to the factory.
        /// </summary>
        /// <param name="services">The <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add logging services to.</param>
        /// <param name="onLog">An action to be invoked when a log message is logged. After inheriting the class from <see cref="IFixtureLogProvider" />, you need to pass <see cref="IFixtureLogProvider.WriteLine" /> to get the action.</param>
        /// <param name="options">An action to be invoked to configure the logger options.</param>
        /// <remarks>This approach is not recommended for integration tests. Use <see cref="XunitForwardingLoggerExtensions.AddXunitForwardingLoggerProvider"/> instead.</remarks>
        /// <returns>The <see cref="Microsoft.Extensions.Logging.ILoggingBuilder" />.</returns>
        public static IServiceCollection AddXunitForwardingLoggerProvider(this IServiceCollection services, LogDelegate? onLog, Action<XunitForwardingLoggerOptions>? options = null)
        {
            var opt = new XunitForwardingLoggerOptions();
            options?.Invoke(opt);

            services.Replace(ServiceDescriptor.Singleton<ILoggerFactory, LoggerFactory>(sp =>
            {
                var loggerFactory = new LoggerFactory();
                loggerFactory.AddProvider(new XunitForwardingLoggerProvider(sp, onLog, opt.MinLevel, opt.IncludeTimestamp));
                return loggerFactory;
            }));

            return services;
        }

        private class XunitForwardingLoggerProvider : ILoggerProvider
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly LogDelegate? _onLog;
            private readonly LogLevel _minLevel;
            private readonly bool _includeTimestamp;

            public XunitForwardingLoggerProvider(IServiceProvider serviceProvider, LogDelegate? onLog, LogLevel minLevel, bool includeTimestamp)
            {
                _serviceProvider = serviceProvider;
                _onLog = onLog;
                _minLevel = minLevel;
                _includeTimestamp = includeTimestamp;
            }

            public ILogger CreateLogger(string categoryName)
            {
                return new XunitForwardingLogger(_serviceProvider, categoryName, _onLog, _minLevel, _includeTimestamp);
            }

            public void Dispose()
            {
            }
        }

        private class XunitForwardingLogger : ILogger, IDisposable
        {
            private readonly string _categoryName;
            private readonly LogDelegate? _onLog;
            private readonly LogLevel _minLevel;
            private readonly bool _includeTimestamp;

            public XunitForwardingLogger(IServiceProvider serviceProvider, string categoryName, LogDelegate? onLog, LogLevel minLevel, bool includeTimestamp)
            {
                _categoryName = categoryName;
                _onLog = onLog;
                _includeTimestamp = includeTimestamp;
                _minLevel = LogProviderHelper.GetMinimumLevel(serviceProvider, _categoryName, minLevel, Enumerable.Empty<string>());
            }

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;

                var log = LogProviderHelper.FormatLog(_categoryName, _includeTimestamp, logLevel, state, exception, formatter);
                if (string.IsNullOrWhiteSpace(log))
                    return;
            
                _onLog?.Invoke(log);
            }
            
            public void Dispose()
            {
            }
        }
    }
}