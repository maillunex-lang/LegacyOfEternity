namespace LegacyOfEternity.Offline
{
    /// <summary>Gates CoastalPrototype gameplay until offline login/hero enter completes.</summary>
    public static class OfflineSession
    {
        public static bool Ready { get; private set; }
        public static OfflineDemoStore.Hero Hero { get; private set; }
        public static string Token { get; private set; }

        public static void Enter(string token, OfflineDemoStore.Hero hero)
        {
            Token = token;
            Hero = hero;
            Ready = hero != null;
        }

        public static void Clear()
        {
            Ready = false;
            Hero = null;
            Token = null;
        }
    }
}
