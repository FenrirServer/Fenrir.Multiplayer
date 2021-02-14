using Fenrir.Multiplayer.Logging;
using Microsoft.Extensions.Logging;
using System;

namespace Fenrir.Multiplayer.Tests.Fixtures
{
    class TestLogger : Logging.ILogger, IDisposable
    {
        private ILoggerFactory _loggerFactory;
        private Microsoft.Extensions.Logging.ILogger _logger;

        public TestLogger()
        {
            _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = _loggerFactory.CreateLogger<TestLogger>();
        }

        public void Critical(string format, params object[] arguments) => _logger.LogCritical(format, arguments);

        public void Debug(string format, params object[] arguments) => _logger.LogDebug(format, arguments);

        public void Error(string format, params object[] arguments) => _logger.LogError(format, arguments);

        public void Info(string format, params object[] arguments) => _logger.LogInformation(format, arguments);

        public void Trace(string format, params object[] arguments) => _logger.LogTrace(format, arguments);

        public void Warning(string format, params object[] arguments) => _logger.LogWarning(format, arguments);

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }

    }
}
