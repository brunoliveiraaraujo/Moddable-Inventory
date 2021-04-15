using System.IO;
using System.Text;
using UnityEngine;

namespace ModdableInventory.Utils
{
    public static class IOUtils
    {
        public static void WriteFileToDirectory(string filePath, byte[] fileData)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (!File.Exists(filePath)) // makes sure not to overwrite existing files
            {
                File.WriteAllBytes(filePath, fileData);
            }
        }

        public static StringReader ReadOrMakeYAMLFile(string internalYAML, string externalYAMLPath)
        {
            if (EditorUtils.IsUnityEditor())
            {
                return new StringReader(internalYAML);
            }
            else
            {
                if (File.Exists(externalYAMLPath))
                {
                    return new StringReader(File.ReadAllText(externalYAMLPath));
                }
                else
                {
                    IOUtils.WriteFileToDirectory(externalYAMLPath, Encoding.ASCII.GetBytes(internalYAML));

                    return new StringReader(internalYAML);
                }
            }
        }
    }
}