using UnityEngine;

namespace MorphController
{
    public static class MorphDebug
    {
        public static void Log(object message, Object context)
        {
            Debug.Log("<color=cyan><b>[MorphController]</b></color>:" + message, context);
        }

        public static void LogWarning(object message, Object context)
        {
            Debug.LogWarning("<color=cyan><b>[MorphController]</b></color>:" + message, context);
        }

        public static void LogError(object message, Object context)
        {
            Debug.LogError("<color=cyan><b>[MorphController]</b></color>:" + message, context);
        }

        public static void LogError(object message)
        {
            Debug.LogError("<color=cyan><b>[MorphController]</b></color>:" + message);
        }
    }
}
