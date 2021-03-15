using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Globalization;

namespace Utils
{
    public static class EditorOperations
    {
        public static void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }

    public static class StringOperations
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
}
