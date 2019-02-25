using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.SharedFiles;

namespace Nop.Data.Mapping.SharedFiles
{
    /// <summary>
    /// Represents a picture mapping configuration
    /// </summary>
    public partial class SharedFileMap : NopEntityTypeConfiguration<SharedFile>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<SharedFile> builder)
        {
            builder.ToTable(nameof(SharedFile));
            builder.HasKey(sharedFile => sharedFile.Id);
            
            builder.Property(sharedFile => sharedFile.FilePath).HasMaxLength(256).IsRequired();
            builder.Property(sharedFile => sharedFile.FileSize).IsRequired();

            base.Configure(builder);
        }

        #endregion
    }
}