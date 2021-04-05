using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ModdableInventory.Utils
{
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
