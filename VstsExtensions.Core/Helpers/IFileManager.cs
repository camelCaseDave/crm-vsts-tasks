using System.IO.Abstractions;

namespace VstsExtensions.Core.Helpers
{
    public interface IFileManager
    {
        IFileSystem FileSystem { get; }

        string GetJsonConfig(string fileName, string directory);

        void StoreJsonConfig(string json, string fileName, string directory);
    }
}