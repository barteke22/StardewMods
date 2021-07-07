using StardewModdingAPI.Utilities;

namespace FishingMinigames
{
    public class ModConfig
    {
        public int VoiceVolume { get; set; } = 100;
        public int VoicePitch { get; set; }
        public string Voice_Test_Ignore_Me { get; set; }//internal for voice setting change check
        public string KeyBinds { get; set; } = "MouseLeft, Space, ControllerX";
        public int StartMinigameStyle { get; set; } = 1;
        public int EndMinigameStyle { get; set; } = 1;
        public float EndMinigameDamage { get; set; } = 1f;
        public int MinigameDifficulty { get; set; } = 0;
        public int FestivalMode { get; set; } = 2;


        public ModConfig()
        {
            int rnd = StardewValley.Game1.random.Next(-70, 71);
            this.VoicePitch = rnd;
            this.Voice_Test_Ignore_Me = "100/" + rnd;
        }
    }
}
