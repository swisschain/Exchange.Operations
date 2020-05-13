using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using OperationsTests.Utils;
using Xunit;

namespace OperationsTests
{
    public class BaseTests
    {
        protected void AssertLoggerWarning(LogLevel logLevel, EventId eventId, object state, Exception e, object callback)
        {
            Assert.Equal(LogLevel.Warning, logLevel);

            var logValues = (IReadOnlyList<KeyValuePair<string, object>>)state;
            var message = string.Join(Environment.NewLine, logValues.Select(x => (string)x.Value).ToList());

            Assert.Contains("The fee is set to 0", message);
        }

        protected void AssertLoggerOnlyInfo(LogLevel logLevel, EventId eventId, object state, Exception e, object callback)
        {
            throw new InvalidOperationException("This is a positive test, there has to be only info logging.");
        }

        protected ILogger<T> InitializeLogger<T>(
            Action<LogLevel, EventId, object, Exception, object> assertMethod, AssertCalls assertCalls)
        {
            var loggerMock = new Mock<ILogger<T>>();

            loggerMock.Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)))
                .Callback((LogLevel logLevel, EventId eventId, object state, Exception e, object callback) =>
                {
                    if (logLevel == LogLevel.Information)
                        return;

                    assertMethod(logLevel, eventId, state, e, callback);

                    assertCalls.Count++;
                });

            return loggerMock.Object;
        }
    }
}
