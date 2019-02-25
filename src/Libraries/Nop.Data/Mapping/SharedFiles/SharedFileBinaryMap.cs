using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.SharedFiles;

namespace Nop.Data.Mapping.SharedFiles
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public partial class SharedFileBinaryMap : NopEntityTypeConfiguration<SharedFileBinary>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<SharedFileBinary> builder)
        {
            builder.ToTable(nameof(SharedFileBinary));
            builder.HasKey(pictureBinary => pictureBinary.Id);
            
            builder.HasOne(sharedFileBinary => sharedFileBinary.SharedFile)
                .WithOne(sharedFile => sharedFile.FileBinary)
                .HasForeignKey<SharedFileBinary>(sharedFileBinary => sharedFileBinary.SharedFileId)
                .OnDelete(DeleteBehavior.Cascade);

            base.Configure(builder);
        }

        #endregion
    }
}