namespace Nop.Core.Domain.SharedFiles
{
    /// <summary>
    /// Represents a file
    /// </summary>
    public partial class SharedFile : BaseEntity
    {
        /// <summary>
        /// Gets or sets the file path
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the file size
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Gets or sets the file binary
        /// </summary>
        public virtual SharedFileBinary FileBinary { get; set; }
    }
}
