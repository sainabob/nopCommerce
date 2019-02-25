using System;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.SharedFiles;
using Nop.Core.Infrastructure;

namespace Nop.Services.Infrastructure
{
    /// <summary>
    /// Sharing file functions using the database
    /// </summary>
    public partial class NopFileDatabaseSharing : INopFileSharing
    {
        private readonly NopConfig _nopConfig;
        private readonly IRepository<SharedFile> _sharedFileRepository;
        private readonly IRepository<SharedFileBinary> _sharedFileBinaryRepository;
        private readonly IStaticCacheManager _cacheManager;

        public NopFileDatabaseSharing(NopConfig nopConfig, IStaticCacheManager cacheManager, IRepository<SharedFile> sharedFileRepository, IRepository<SharedFileBinary> sharedFileBinaryRepository)
        {
            _nopConfig = nopConfig;
            _sharedFileRepository = sharedFileRepository;
            _sharedFileBinaryRepository = sharedFileBinaryRepository;
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Updates the shared file binary data
        /// </summary>
        /// <param name="sharedFile">The shared file object</param>
        /// <param name="binaryData">The shared file binary data</param>
        /// <returns>Shared file binary</returns>
        protected virtual SharedFileBinary UpdateSharedFileBinary(SharedFile sharedFile, byte[] binaryData)
        {
            if (sharedFile == null)
                throw new ArgumentNullException(nameof(sharedFile));

            var sharedFileBinary = sharedFile.FileBinary;

            var isNew = sharedFileBinary == null;

            if (isNew)
                sharedFileBinary = new SharedFileBinary
                {
                    SharedFileId = sharedFile.Id
                };

            sharedFileBinary.BinaryData = binaryData;

            if (isNew)
                _sharedFileBinaryRepository.Insert(sharedFileBinary);
            else
                _sharedFileBinaryRepository.Update(sharedFileBinary);

            return sharedFileBinary;
        }

        /// <summary>
        /// Gets a value indicating whether file sharing is enable
        /// </summary>
        public bool IsEnable => _nopConfig.SharingFile && DataSettingsManager.DatabaseIsInstalled;

        /// <summary>
        /// Determines whether the specified file exists
        /// </summary>
        /// <param name="filePath">The file to check</param>
        /// <returns>True if the caller has the required permissions and path contains the name of an existing file; otherwise, false.</returns>
        public bool FileExists(string filePath)
        {
            if (!IsEnable)
                return false;

            if (_cacheManager.IsSet(filePath))
            {
                return true;
            }

            var file = _cacheManager.Get(filePath, () =>
            {
                return _sharedFileRepository.Table.FirstOrDefault(sharedFile =>
                    sharedFile.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase));
            });

            if (file == null)
            {
                _cacheManager.Remove(filePath);
            }

            return file != null;
        }

        /// <summary>
        /// Get a file binary data 
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>File binary data</returns>
        public byte[] GetBinaryData(string filePath)
        {
            if(!FileExists(filePath))
                return new byte[0];

            return _cacheManager.Get<SharedFile>(filePath, () => null)?.FileBinary?.BinaryData ?? new byte[0];
        }

        /// <summary>
        /// Sharing a file
        /// </summary>
        /// <param name="filePath">The path of file for sharing</param>
        /// <param name="binaryData">The file binary data</param>
        public void Share(string filePath, byte[] binaryData)
        {
            if (!IsEnable)
                return;

            var file = _cacheManager.Get(filePath, () =>
            {
                var sharedFile = _sharedFileRepository.Table.FirstOrDefault(sf =>
                    sf.FilePath.Equals(filePath, StringComparison.InvariantCultureIgnoreCase));

                if (sharedFile != null)
                    return sharedFile;

                sharedFile = new SharedFile
                {
                    FilePath = filePath,
                    FileSize = binaryData.LongLength
                };

                _sharedFileRepository.Insert(sharedFile);

                return sharedFile;
            });

            UpdateSharedFileBinary(file, binaryData);
        }
    }
}
