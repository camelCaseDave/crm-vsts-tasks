using Newtonsoft.Json;
using System.IO.Abstractions;
using VstsExtensions.Core.Models;

namespace VstsExtensions.Core.Helpers
{
    public class FileManager : IFileManager
    {
        public IFileSystem FileSystem { get; private set; }

        public FileManager(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
        }

        public FileManager()
        {
            FileSystem = new FileSystem();
        }

        public string GetJsonConfig(string fileName, string directory)
        {
            return FileSystem.File.ReadAllText($"{directory}\\{fileName}.json");
        }

        public void StoreJsonConfig(string json, string fileName, string directory)
        {
            FileSystem.File.WriteAllText($"{directory}\\{fileName}.json", json.ToString());
        }

        public void WriteDataToFile(ExportedData data, string directory)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.None
            });

            StoreJsonConfig(json, "ExportedData", directory);
        }
    }
}