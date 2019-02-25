namespace Nop.Core.Infrastructure
{
    /// <summary>
    /// A file sharing abstraction
    /// </summary>
    public interface INopFileSharing
    {
        /// <summary>
        /// Determines whether the specified file exists
        /// </summary>
        /// <param name="filePath">The file to check</param>
        /// <returns>True if the caller has the required permissions and path contains the name of an existing file; otherwise, false.</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// Get a file binary data 
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>File binary data</returns>
        byte[] GetBinaryData(string filePath);

        /// <summary>
        /// Sharing a file
        /// </summary>
        /// <param name="filePath">The path of file for sharing</param>
        /// <param name="binaryData">The file binary data</param>
        void Share(string filePath, byte[] binaryData);

        /// <summary>
        /// Gets a value indicating whether file sharing is enable
        /// </summary>
        bool IsEnable { get; }
    } 
}
