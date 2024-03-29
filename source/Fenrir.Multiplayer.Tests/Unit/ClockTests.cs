﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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

            var data = new[]
            {
                new { ClockOffset = 52, ProcessingTime = 5, RoundTrip = 90 },
                new { ClockOffset = 55, ProcessingTime = 4, RoundTrip = 100 },
                new { ClockOffset = 45, ProcessingTime = 6, RoundTrip = 110 },
                new { ClockOffset = 48, ProcessingTime = 5, RoundTrip = 80 },
            };

            DateTime timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse = DateTime.MinValue;

            for (int i = 0; i < data.Length; i++)
            {
                var syncData = data[i];
                int startTime = i * 1000;

                clientTime = serverTime + TimeSpan.FromMilliseconds(syncData.ClockOffset);
                timeSentRequest = clientTime + TimeSpan.FromMilliseconds(startTime);
                timeReceivedRequest = serverTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip / 2 - syncData.ProcessingTime / 2);
                timeSentResponse = serverTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip / 2 + syncData.ProcessingTime / 2);
                timeReceivedResponse = clientTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip);
                clockSynchronizer.RecordSyncResult(timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse);
            }

            // Verify average time difference - 50ms
            Assert.AreEqual(TimeSpan.FromMilliseconds(-50), clockSynchronizer.AvgOffset);


            // Calculate expected coefficient of variation
            int sumRoundTrips = 90 + 100 + 110 + 80; // 380
            int roundTripAvg = sumRoundTrips / 4; // 95
            double variance = (Math.Pow(90 - roundTripAvg, 2) + Math.Pow(100 - roundTripAvg, 2) + Math.Pow(110 - roundTripAvg, 2) + Math.Pow(80 - roundTripAvg, 2)) / 4; // 125
            double deviation = Math.Sqrt(variance); // ~11.18
            double variationCoefficient = deviation / roundTripAvg; // ~0.12
            double expectedNextDelayMs = Lerp(clockSynchronizer.MinSyncDelay.TotalMilliseconds, clockSynchronizer.MaxSyncDelay.TotalMilliseconds, 1 - variationCoefficient); // 8.881 sec

            Assert.AreEqual(expectedNextDelayMs, (clockSynchronizer.NextSyncTime - clockSynchronizer.LastSyncTime).TotalMilliseconds, 1d);
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


        [TestMethod]
        public void ClockSynchronizer_RecordSyncResult_RemovesOldData()
        {
            var clockSynchronizer = new ClockSynchronizer()
            {
                RoundTripsMaxSampleSize = 4,
                TimeOffsetsMaxSampleSize = 4
            };

            DateTime serverTime = DateTime.UtcNow;
            DateTime clientTime;

            var data = new[]
            {
                 // this data point should be removed, so avg offset will be 50
                new { ClockOffset = 52, ProcessingTime = 5, RoundTrip = 90 },

                new { ClockOffset = 52, ProcessingTime = 5, RoundTrip = 90 },
                new { ClockOffset = 55, ProcessingTime = 4, RoundTrip = 100 },
                new { ClockOffset = 45, ProcessingTime = 6, RoundTrip = 110 },
                new { ClockOffset = 48, ProcessingTime = 5, RoundTrip = 80 },
            };

            DateTime timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse;

            for (int i = 0; i < data.Length; i++)
            {
                var syncData = data[i];
                int startTime = i * 1000;

                clientTime = serverTime + TimeSpan.FromMilliseconds(syncData.ClockOffset);
                timeSentRequest = clientTime + TimeSpan.FromMilliseconds(startTime);
                timeReceivedRequest = serverTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip / 2 - syncData.ProcessingTime / 2);
                timeSentResponse = serverTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip / 2 + syncData.ProcessingTime / 2);
                timeReceivedResponse = clientTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip);
                clockSynchronizer.RecordSyncResult(timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse);
            }

            // Verify average time difference - 50ms. Outliers are ignored
            Assert.AreEqual(TimeSpan.FromMilliseconds(-50), clockSynchronizer.AvgOffset);
        }


        [TestMethod]
        public void ClockSynchronizer_RecordSyncResult_SoakTest()
        {
            var data = new[]
            {
                new { ClockOffset = 52, ProcessingTime = 5, RoundTrip = 90 },
                new { ClockOffset = 55, ProcessingTime = 4, RoundTrip = 100 },
                new { ClockOffset = 45, ProcessingTime = 6, RoundTrip = 110 },
                new { ClockOffset = 48, ProcessingTime = 5, RoundTrip = 80 },
            };

            int numIterations = 200;

            // Sample size should be a multiple of data.Length for this test, otherwise expected values will be slightly off because some samples will be lost
            var clockSynchronizer = new ClockSynchronizer() { TimeOffsetsMaxSampleSize = 40, RoundTripsMaxSampleSize = 40 } ;
            DateTime serverTime = DateTime.UtcNow;
            DateTime clientTime;

            DateTime timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse = DateTime.MinValue;

            for (int i = 0; i < numIterations * data.Length; i++)
            {
                var syncData = data[i % data.Length];
                int startTime = i * 1000;

                clientTime = serverTime + TimeSpan.FromMilliseconds(syncData.ClockOffset);
                timeSentRequest = clientTime + TimeSpan.FromMilliseconds(startTime);
                timeReceivedRequest = serverTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip / 2 - syncData.ProcessingTime / 2);
                timeSentResponse = serverTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip / 2 + syncData.ProcessingTime / 2);
                timeReceivedResponse = clientTime + TimeSpan.FromMilliseconds(startTime + syncData.RoundTrip);
                clockSynchronizer.RecordSyncResult(timeSentRequest, timeReceivedRequest, timeSentResponse, timeReceivedResponse);
            }

            // Verify average time difference - 50ms
            Assert.AreEqual(TimeSpan.FromMilliseconds(-50).TotalMilliseconds, clockSynchronizer.AvgOffset.TotalMilliseconds, 1d);


            // Calculate expected coefficient of variation
            int sumRoundTrips = (90 + 100 + 110 + 80); // 380
            int roundTripAvg = sumRoundTrips / 4; // 95
            double variance = (Math.Pow(90 - roundTripAvg, 2) + Math.Pow(100 - roundTripAvg, 2) + Math.Pow(110 - roundTripAvg, 2) + Math.Pow(80 - roundTripAvg, 2)) / 4; // 125
            double deviation = Math.Sqrt(variance); // ~11.18
            double variationCoefficient = deviation / roundTripAvg; // ~0.12
            double expectedNextDelayMs = Lerp(clockSynchronizer.MinSyncDelay.TotalMilliseconds, clockSynchronizer.MaxSyncDelay.TotalMilliseconds, 1 - variationCoefficient); // 8.881 sec

            Assert.AreEqual(expectedNextDelayMs, (clockSynchronizer.NextSyncTime - clockSynchronizer.LastSyncTime).TotalMilliseconds, 1d);
        }

        private double Lerp(double a, double b, double x)
        {
            return a * (1 - x) + b * x;
        }
    }
}
