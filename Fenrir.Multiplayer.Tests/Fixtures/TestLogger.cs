using Fenrir.Multiplayer.Logging;
using Microsoft.Extensions.Logging;
using System;

namespace Fenrir.Multiplayer.Tests.Fixtures
{
    class TestLogger : IFenrirLogger
    {
        private ILogger _logger;

        public TestLogger()
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<TestLogger>();
        }

        public void Critical(string format, params object[] arguments) => _logger.LogCritical(format, arguments);

        public void Debug(string format, params object[] arguments) => _logger.LogDebug(format, arguments);

        public void Error(string format, params object[] arguments) => _logger.LogError(format, arguments);

        public void Info(string format, params object[] arguments) => _logger.LogInformation(format, arguments);

        public void Trace(string format, params object[] arguments) => _logger.LogTrace(format, arguments);

        public void Warning(string format, params object[] arguments) => _logger.LogWarning(format, arguments);
    }
}
