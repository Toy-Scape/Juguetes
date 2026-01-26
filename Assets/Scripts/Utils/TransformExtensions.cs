using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class TransformExtensions
{
    public static Transform FindDeep (this Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            var result = child.FindDeep(name);

            if (result != null)
                return result;
        }
        return null;
    }
}
