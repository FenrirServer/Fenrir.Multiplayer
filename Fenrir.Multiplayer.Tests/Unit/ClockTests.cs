using Fenrir.Multiplayer.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Tests.Unit
{
    [TestClass]
    public class ClockTests
    {
        [TestMethod]
        public void ClockSynchronizer_RecordSyncResult_TracksClockOffset()
        {
            var clockSynchronizer = new ClockSynchronizer();
            DateTime serverTime = DateTime.UtcNow;
            DateTime clientTime;

            DateTime timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse;

            // Sync 0 - difference 52ms, processing 5ms, round trip 90ms
            clientTime = serverTime + TimeSpan.FromMilliseconds(52);
            timeSentRequest = clientTime + TimeSpan.FromMilliseconds(0);
            timeReceivedRequest = serverTime + TimeSpan.FromMilliseconds(45);
            timeSentResponse = serverTime + TimeSpan.FromMilliseconds(45);
            timeReceivedResponse = clientTime + TimeSpan.FromMilliseconds(90);
            clockSynchronizer.RecordSyncResult(timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse);

            // Sync 1 - difference 55ms, processing 4ms, round trip 100ms
            clientTime = serverTime + TimeSpan.FromMilliseconds(55);
            timeSentRequest = clientTime + TimeSpan.FromMilliseconds(100);
            timeReceivedRequest = serverTime + TimeSpan.FromMilliseconds(150);
            timeSentResponse = serverTime + TimeSpan.FromMilliseconds(150);
            timeReceivedResponse = clientTime + TimeSpan.FromMilliseconds(200);
            clockSynchronizer.RecordSyncResult(timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse);

            // Sync 2 - difference 45ms, processing 6ms, round trip 110ms
            clientTime = serverTime + TimeSpan.FromMilliseconds(45);
            timeSentRequest = clientTime + TimeSpan.FromMilliseconds(200);
            timeReceivedRequest = serverTime + TimeSpan.FromMilliseconds(255);
            timeSentResponse = serverTime + TimeSpan.FromMilliseconds(255);
            timeReceivedResponse = clientTime + TimeSpan.FromMilliseconds(310);
            clockSynchronizer.RecordSyncResult(timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse);

            // Sync 3 - difference 48ms, processing 5ms, round trip 80ms
            clientTime = serverTime + TimeSpan.FromMilliseconds(48);
            timeSentRequest = clientTime + TimeSpan.FromMilliseconds(300);
            timeReceivedRequest = serverTime + TimeSpan.FromMilliseconds(340);
            timeSentResponse = serverTime + TimeSpan.FromMilliseconds(340);
            timeReceivedResponse = clientTime + TimeSpan.FromMilliseconds(380);
            clockSynchronizer.RecordSyncResult(timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse);

            // Verify average time difference - 50ms
            Assert.AreEqual(TimeSpan.FromMilliseconds(-50), clockSynchronizer.AvgOffset);

            // Verify next delay

            // Calculate expected coefficient of variation
            int sumRoundTrips = 90 + 100 + 110 + 80; // 380
            int roundTripAvg = sumRoundTrips / 4; // 95
            double variance = (Math.Pow(90 - roundTripAvg, 2) + Math.Pow(100 - roundTripAvg, 2) + Math.Pow(110 - roundTripAvg, 2) + Math.Pow(80 - roundTripAvg, 2)) / 4; // 125
            double deviation = Math.Sqrt(variance); // ~11.18
            double variationCoefficient = deviation / roundTripAvg; // ~0.12
            double expectedNextDelay = Lerp(clockSynchronizer.MinSyncDelay.TotalMilliseconds, clockSynchronizer.MaxSyncDelay.TotalMilliseconds, variationCoefficient); // 1.62 sec

            Assert.AreEqual((timeReceivedResponse + TimeSpan.FromMilliseconds(expectedNextDelay)).Ticks, clockSynchronizer.NextSyncTime.Ticks, TimeSpan.TicksPerMillisecond);
        }

        [TestMethod]
        public void ClockSynchronizer_RecordSyncResult_IgnoresOutlier()
        {
            var clockSynchronizer = new ClockSynchronizer();
            DateTime serverTime = DateTime.UtcNow;
            DateTime clientTime;

            var data = new[]
            {
                new { ClockOffset = 52, ProcessingTime = 5, RoundTrip = 90 },
                new { ClockOffset = 55, ProcessingTime = 4, RoundTrip = 100 },
                new { ClockOffset = 45, ProcessingTime = 6, RoundTrip = 110 },
                new { ClockOffset = 48, ProcessingTime = 5, RoundTrip = 80 },
                new { ClockOffset = 52, ProcessingTime = 5, RoundTrip = 90 },
                new { ClockOffset = 55, ProcessingTime = 4, RoundTrip = 100 },
                new { ClockOffset = 45, ProcessingTime = 6, RoundTrip = 110 },
                new { ClockOffset = 48, ProcessingTime = 5, RoundTrip = 80 },
                new { ClockOffset = 52, ProcessingTime = 5, RoundTrip = 90 },
                new { ClockOffset = 55, ProcessingTime = 4, RoundTrip = 100 },
                new { ClockOffset = 45, ProcessingTime = 6, RoundTrip = 110 },
                new { ClockOffset = 48, ProcessingTime = 5, RoundTrip = 80 },
                new { ClockOffset = 200, ProcessingTime = 50, RoundTrip = 200 }, // outlier
                new { ClockOffset = 10, ProcessingTime = 1, RoundTrip = 5 }, // outlier
            };

            DateTime timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse;

            for (int i = 0; i < data.Length; i++)
            {
                var syncData = data[i];
                int startTime = i * 1000;

                clientTime = serverTime + TimeSpan.FromMilliseconds(syncData.ClockOffset);
                timeSentRequest = clientTime + TimeSpan.FromMilliseconds(startTime);
                timeReceivedRequest = serverTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip/2 - syncData.ProcessingTime/2);
                timeSentResponse = serverTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip / 2 + syncData.ProcessingTime/2);
                timeReceivedResponse = clientTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip);
                clockSynchronizer.RecordSyncResult(timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse);
            }

            // Verify average time difference - 50ms. Outliers are ignored
            Assert.AreEqual(TimeSpan.FromMilliseconds(-50), clockSynchronizer.AvgOffset);
        }

        private double Lerp(double a, double b, double x)
        {
            return a * (1 - x) + b * x;
        }
    }
}
