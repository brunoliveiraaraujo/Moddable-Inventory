using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// TODO: UI: detect error and show this on UI: 
// "Error: check Player.Log" for more details, in 
//      <user>\AppData\LocalLow\ModdableInventoryDemo\ModdableInventory\
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
