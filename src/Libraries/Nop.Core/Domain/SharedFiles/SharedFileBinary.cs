namespace Nop.Core.Domain.SharedFiles
{
    /// <summary>
    /// Represents a file binary data
    /// </summary>
    public partial class SharedFileBinary : BaseEntity
    {
        /// <summary>
        /// Gets or sets the picture binary
        /// </summary>
        public byte[] BinaryData { get; set; }
       
        /// <summary>
        /// Gets or sets the shared file identifier
        /// </summary>
        public int SharedFileId { get; set; }

        /// <summary>
        /// Gets or sets the shared file
        /// </summary>
        public virtual SharedFile SharedFile { get; set; }
    }
}
