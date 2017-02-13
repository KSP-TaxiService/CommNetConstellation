namespace CommNetConstellation
{
    /// <summary>
    /// Debug-purpose logging
    /// </summary>
    public class CNCLog
    {
        public static readonly string NAME_LOG_PREFIX = "CommNet Constellation";

        public static void Verbose(string message, params object[] param)
        {
            UnityEngine.Debug.Log(string.Format("{0} -> verbose: {1}", NAME_LOG_PREFIX, string.Format(message, param)));
        }

        public static void Debug(string message, params object[] param)
        {
#if DEBUG
            UnityEngine.Debug.Log(string.Format("{0} -> debug: {1}", NAME_LOG_PREFIX, string.Format(message, param)));
#endif
        }

        public static void Error(string message, params object[] param)
        {
            UnityEngine.Debug.LogError(string.Format("{0} -> error: {1}", NAME_LOG_PREFIX, string.Format(message, param)));
        }
    }
}
