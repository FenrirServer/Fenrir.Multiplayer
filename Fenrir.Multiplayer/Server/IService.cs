using System.Threading.Tasks;

namespace Fenrir.Multiplayer.Server
{
    /// <summary>
    /// Represents extension service that can be added 
    /// to Network Server
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Indicates if service is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the service.
        /// This method is invoked when NetworkServer is starting, before any protocols are initialized
        /// </summary>
        /// <returns>Task that must complete when service has started. Failing this task will fail <see cref="NetworkServer.Start"/></returns>
        Task Start();

        /// <summary>
        /// Stops the service.
        /// This method is invoked when NetworkServer is stopping, after any protocols are stopped.
        /// </summary>
        /// <returns>Task that must complete when service has stopped. Failing this task will fail <see cref="NetworkServer.Stop"/></returns>
        Task Stop();
    }
}