using System;
using UnityEngine;

namespace MorphController
{
    [Serializable]
    public class MorphCallBack
    {
        public GameObject CallTarget;
        public string CallMethod;

        public MorphCallBack()
        {
            CallTarget = null;
            CallMethod = "<None>";
        }
    }
}
