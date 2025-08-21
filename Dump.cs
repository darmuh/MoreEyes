using MoreEyes.EyeManagement;
using System.Collections.Generic;
using UnityEngine;

namespace MoreEyes;
// Throwing some stuff here that are not used anymore but could be used later? We can delete this class once we are ready to release
internal class Dump
{
    //not used but may be useful at some point
    private static Transform RecursiveFindMatchingChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(childName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                return child;
            }
            else
            {
                Transform found = RecursiveFindMatchingChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }
    // placeholders so they wont spam errors
    private bool inUse;
    private static readonly List<CustomIrisType> IrisInUse = [];
    private readonly CustomIrisType that = null!; // placeholder for "this"


    //note sure if we are gonna use this to be honest
    internal void MarkIrisUnused()
    {
        //might just use this lol
        inUse = false;

        if (IrisInUse.Contains(that))
        {
            IrisInUse.Remove(that);
        }
    }
}
