using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Globalization;
using System.IO;

namespace Utils
{
    public static class StringUtils
    {
        public static string FloatToString(float number, int precision)
        {
            return number.ToString($"F{precision.ToString()}", CultureInfo.InvariantCulture);
        }

        public static string NoSpacesAndLowerCaseString(string str)
        {
            return str.Replace(" ", "").ToLower();
        }
    }

    public static class IOUtils
    {
        public static void WriteFileToDirectory(string filePath, byte[] fileData)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (!File.Exists(filePath)) // make sure not to overwrite
            {
                File.WriteAllBytes(filePath, fileData);
            }
        }
    }

    public static class EditorUtils
    {
        public static bool IsUnityEditor()
        {
            #if UNITY_EDITOR
                return true;
            #else
                return false;
            #endif
        }
    }
}
