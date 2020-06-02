using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utility
{

    public static class GameObjectUtility
    {
        public static GameObject FindChild(this GameObject obj, string name)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                if (obj.transform.GetChild(i).gameObject.name == name)
                {
                    return obj.transform.GetChild(i).gameObject;
                }
            }
            throw new System.Exception("No child named " + name + " could be found on object " + obj.name);
        }
    }
}