using System;

namespace ModdableInventory.Utils
{
    public static class ClassUtils
    {
        public static bool IsSameOrSubclass(Type potentialBase, Type potentialSub)
        {
            return potentialSub.IsSubclassOf(potentialBase) || potentialSub == potentialBase;
        }
    }
}