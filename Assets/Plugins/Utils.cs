using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Globalization;
using System.IO;
using System.Text;

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
        public static void WriteFileToDirectory(string filePath, string fileData)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllBytes(filePath, Encoding.ASCII.GetBytes(fileData));
        }
    }
}
