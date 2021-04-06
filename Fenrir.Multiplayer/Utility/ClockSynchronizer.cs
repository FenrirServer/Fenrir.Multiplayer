using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fenrir.Multiplayer.Utility
{
    /// <summary>
    /// Utility class to synchronize offset between two clocks.
    /// </summary>
    /// <remarks>
    /// Clock offset and round-trip times are based on the request/response timestamps.
    /// Round-trip time Standard deviation is used to detect and filter out outliers.
    /// Provides next recommended sync time based on the round-trip Coefficient of variation
    /// <seealso cref="https://en.wikipedia.org/wiki/Network_Time_Protocol"/>
    /// <seealso cref="https://en.wikipedia.org/wiki/Standard_deviation"/>
    /// <seealso cref="https://en.wikipedia.org/wiki/Coefficient_of_variation"/>
    /// </remarks>
    /// <example>
    /// Assuming sample recorded round-trip values: [90, 100, 110, 80] 
    /// Sum: 90+100+110+80 = 380
    /// Avg: 380/4=95
    /// Deviations from avg for each value: [-5, 5, 15, -15]
    /// Squared deviations: [25, 25, 225, 225]
    /// Variance (mean, or average of squared deviations): (25+25+225+225)/4 = 125
    /// Standard deviation: Sqrt(125) = ~11.18
    /// Coefficient of variation: 11.180/95 = ~0.1176
    /// 
    /// Next sync delay is a Coefficient of variation lerped between <see cref="MinSyncDelay"/> and <see cref="MaxSyncDelay"/>:
    /// Delay = 0.5 * (1 - 0.1176) + 10 * 0.1176 = 1.62 sec
    /// 
    /// If next ping comes in as a [150], it will be detected as an outlier:
    /// Deviation from avg: 150-95=55
    /// 55 > ~11.18 * 2 (deviation is more than twice the standard deviation e.g. exceeds the outlier threshold)
    /// </example>
    class ClockSynchronizer
    {
        /// <summary>
        /// How many round-trip values to store maximum
        /// </summary>
        public int RoundTripsMaxSampleSize { get; set; } = 25;

        /// <summary>
        /// How manu time offset values to store maximum
        /// </summary>
        public int TimeOffsetsMaxSampleSize { get; set; } = 25;

        /// <summary>
        /// Minimal delay between clock synchronization attempts
        /// </summary>
        public TimeSpan MinSyncDelay { get; set; } = TimeSpan.FromSeconds(0.5f);

        /// <summary>
        /// Maximum delay between clock synchronization attempts
        /// </summary>
        public TimeSpan MaxSyncDelay { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Current average offset between local and remote clock
        /// </summary>
        public TimeSpan AvgOffset => _clockOffsets.Count == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(_clockOffsetSumTicks / _clockOffsets.Count);

        /// <summary>
        /// Next recommended clock sync time
        /// </summary>
        /// <remarks>
        /// Calculated as a round trip time variation coefficient, interpolated and clamped between <seealso cref="MinSyncDelay"/> and <seealso cref="MaxSyncDelay"/>
        /// </remarks>
        public DateTime NextSyncTime
        {
            get
            {
                long roundTripVariationCoefficientLerped = (long)Lerp(MinSyncDelay.Ticks, MaxSyncDelay.Ticks, 1 - _roundTripVariationCoefficient);
                long syncDelayClamped = Math.Min(Math.Max(MinSyncDelay.Ticks, roundTripVariationCoefficientLerped), MaxSyncDelay.Ticks);
                return _lastSyncTime + TimeSpan.FromTicks(syncDelayClamped);
            }
        }

        /// <summary>
        /// Average round trip time between server and client
        /// </summary>
        public TimeSpan AvgRoundTripTime => _roundTrips.Count == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(_roundTripSum / _roundTrips.Count);

        /// <summary>
        /// Lower bound threshold for a round-trip outlier detection.
        /// This value is a multiplier for the standard deviation
        /// E.g. if standard deviation is +-15ms, and threshold is 2f,
        /// anything round-trip value with deviation from average +-30 will be considered an outlier
        /// </summary>
        public float RoundTripOutlierThreshold { get; set; } = 2f;

        /// <summary>
        /// Number of recorded round trips
        /// </summary>
        public int NumRoundTripsRecorded => _roundTrips.Count;

        /// <summary>
        /// Recorded round trip times, in DateTime ticks
        /// </summary>
        private Queue<long> _roundTrips = new Queue<long>();

        /// <summary>
        /// Total sum of all roundtrip delays we have received from clock sync
        /// </summary>
        private long _roundTripSum = 0;

        /// <summary>
        /// Total sum of all round-trip deviatons from average
        /// Same thing as round-trip variance, multiplied by number of received values
        /// </summary>
        private long _roundTripSumSquaredDeviations = 0;

        /// <summary>
        /// Round trip time standard deviation
        /// </summary>
        private long _roundTripStandardDeviation = 0;

        /// <summary>
        /// Rond trip time variation coefficient
        /// </summary>
        private double _roundTripVariationCoefficient = 0;

        /// <summary>
        /// Linked list of clock offsets recorded
        /// </summary>
        private Queue<long> _clockOffsets = new Queue<long>();

        /// <summary>
        /// Total sum of all offsets we have received from clock sync
        /// </summary>
        private long _clockOffsetSumTicks = 0;

        /// <summary>
        /// Time when last data from the sample was recorded
        /// </summary>
        private DateTime _lastSyncTime;

        /// <summary>
        /// Recurds clock synchronization result data
        /// </summary>
        /// <param name="timeSentRequest">Time when sync request was sent by the local party</param>
        /// <param name="timeReceivedRequest">Time when sync request was received by remote party</param>
        /// <param name="timeSentResponse">Time when sync response was sent by remote party</param>
        /// <param name="timeReceivedResponse">Time when sync was received by local party</param>
        public void RecordSyncResult(DateTime timeSentRequest, DateTime timeReceivedRequest, DateTime timeSentResponse, DateTime timeReceivedResponse)
        {
            long roundTripTimePrevAvg = _roundTrips.Count == 0 ? 0 : _roundTripSum / _roundTrips.Count;

            // Check if we have room for another value, if not, remove the oldest value we have
            if (_roundTrips.Count == RoundTripsMaxSampleSize) // Have reached max size, remove oldest value
            {
                // Remove oldest round-trip value
                long oldestRoundTrip = _roundTrips.Dequeue();

                // Remove from the sum (used to calculate avg)
                _roundTripSum -= oldestRoundTrip;

                // Remove from the sum of squared deviations
                long oldestRoundTripDeviation = oldestRoundTrip - roundTripTimePrevAvg;
                long oldestRoundTripDeviationSquared = oldestRoundTripDeviation * oldestRoundTripDeviation;
                _roundTripSumSquaredDeviations -= (oldestRoundTripDeviationSquared * oldestRoundTripDeviationSquared);
            }

            // Calculate new round-trip value
            TimeSpan roundTripTime = timeReceivedResponse - timeSentRequest;

            // Record newly received round-trip time
            _roundTrips.Enqueue(roundTripTime.Ticks);
            _roundTripSum += roundTripTime.Ticks;

            // Record received round-trip squared deviation from the average round-trip time
            long roundTripTimeAvg = _roundTripSum / _roundTrips.Count;
            long roundTripDeviation = roundTripTime.Ticks - roundTripTimeAvg;

            // Calculate standard deviation for the whole sample (all values) using Welford's method

            // Calculate difference between old and new sum of squared deviations
            long roundTripSumSquaredDeviationsDiff = (roundTripTime.Ticks - roundTripTimeAvg) * (roundTripTime.Ticks - roundTripTimePrevAvg);

            // Calculate new variance
            _roundTripSumSquaredDeviations += roundTripSumSquaredDeviationsDiff;
            long roundTripTotalVariance = _roundTripSumSquaredDeviations / _roundTrips.Count;


            _roundTripStandardDeviation = (long)Math.Sqrt(roundTripTotalVariance);
            _roundTripVariationCoefficient = (double)_roundTripStandardDeviation / roundTripTimeAvg;

            if (IsRoundTripOutlier(roundTripDeviation))
            {
                return; // Do not record time offset
            }

            // Record time offset

            // Check if we have room for another offset, if not, remove the oldest clock offset we have
            if (_clockOffsets.Count == TimeOffsetsMaxSampleSize) // Have reached max size, remove oldest value
            {
                // Remove oldest offset value
                long oldestTimeOffset = _clockOffsets.Dequeue();
                _clockOffsetSumTicks -= oldestTimeOffset;
            }

            long clockOffset = ((timeReceivedRequest - timeSentRequest) + (timeSentResponse - timeReceivedResponse)).Ticks / 2;
            _clockOffsets.Enqueue(clockOffset);
            _clockOffsetSumTicks += clockOffset; // Set clock offset

            _lastSyncTime = timeReceivedResponse;
        }

        /// <summary>
        /// Returns true if recorded value is an outlier based on the round trip deviation
        /// compared to standard deviation of the whole sample.
        /// </summary>
        /// <param name="roundTripDeviation">Deviation from the average: (value - avg)</param>
        /// <returns>True if round-trip time is an outlier, otherwise false</returns>
        private bool IsRoundTripOutlier(long roundTripDeviation)
        {
            // Check if current round trip deviation is too big, if so, do not record this as an time offset.
            // For this case too big means smaller than half the standard deviation or bigger than twice the standard deviation
            // We do not want this outlier to affect our datetime offset
            return Math.Abs(roundTripDeviation) > _roundTripStandardDeviation * RoundTripOutlierThreshold;
        }


        private double Lerp(double a, double b, double x)
        {
            return a * (1 - x) + b * x;
        }
    }
}
