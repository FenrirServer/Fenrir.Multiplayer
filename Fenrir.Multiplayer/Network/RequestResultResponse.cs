using Fenrir.Multiplayer.Serialization;

namespace Fenrir.Multiplayer.Network
{
    /// <summary>
    /// Basic response to any request
    /// </summary>
    public class RequestResultResponse : IResponse, IByteStreamSerializable
    {
        /// <summary>
        /// Returns successful response
        /// </summary>
        public static RequestResultResponse SuccessfulResponse => new RequestResultResponse(true);

        /// <summary>
        /// Indicates if response completed successfully, otherwise false
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// If requested operation was not executed successfully, might contain a numeric error code to pass to the client
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// If requested operation was not executed successfully, might contain text description of the reason
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Creates room leave response
        /// </summary>
        public RequestResultResponse()
            : this(true)
        {
        }

        /// <summary>
        /// Creates room leave response
        /// </summary>
        /// <param name="success">True if left successfully, otherwise false</param>
        public RequestResultResponse(bool success)
        {
            Success = success;
        }

        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="success">True if left successfully, otherwise false</param>
        /// <param name="errorCode">If success is false, provides numeric code to indicate why leave attempt was unsuccessful</param>
        public RequestResultResponse(bool success, int errorCode)
            : this(success)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="success">True if left successfully, otherwise false</param>
        /// <param name="errorCode">If success is false, provides numeric code to indicate why leave attempt was unsuccessful</param>
        /// <param name="reason">If success is false, provides text reason why leave attempt was unsuccessful</param>
        public RequestResultResponse(bool success, int errorCode, string reason)
            : this(success, errorCode)
        {
            Reason = reason;
        }

        /// <summary>
        /// Factory method - creates successful <see cref="RequestResultResponse"/>
        /// </summary>
        /// <returns>Successful <see cref="RequestResultResponse"/></returns>
        public static RequestResultResponse FromSuccess()
        {
            return new RequestResultResponse(true);
        }

        /// <summary>
        /// Factory method - creates failed <see cref="RequestResultResponse"/>
        /// </summary>
        /// <param name="errorCode">Numeric code to indicate why requested operation did not succeed</param>
        /// <returns>Failed <see cref="RequestResultResponse"/></returns>
        public static RequestResultResponse FromFailure(int errorCode)
        {
            return new RequestResultResponse(false, errorCode);
        }

        /// <summary>
        /// Factory method - creates failed <see cref="RequestResultResponse"/>
        /// </summary>
        /// <param name="reason">Text reason why requested operation did not succeed</param>
        /// <param name="errorCode">Numeric code to indicate why requested operation did not succeed</param>
        /// <returns>Failed <see cref="RequestResultResponse"/></returns>
        public static RequestResultResponse FromFailure(int errorCode, string reason)
        {
            return new RequestResultResponse(false, errorCode, reason);
        }

        #region IByteStreamSerializable Implementation
        void IByteStreamSerializable.Deserialize(IByteStreamReader reader)
        {
            Success = reader.ReadBool();

            if (!Success && !reader.EndOfData)
            {
                ErrorCode = reader.ReadInt();
            }

            if (!Success && !reader.EndOfData)
            {
                Reason = reader.ReadString();
            }
        }

        void IByteStreamSerializable.Serialize(IByteStreamWriter writer)
        {
            writer.Write(Success);

            if (!Success)
            {
                writer.Write(ErrorCode);
            }

            if (!Success && Reason != null)
            {
                writer.Write(Reason);
            }
        }
        #endregion
    }
}
