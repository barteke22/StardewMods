namespace FishingInfoOverlays
{
    internal class ModConfig
    {
        public int BarSonarMode { get; set; } = 2;
        public int[] BarIconMode { get; set; } = [0, 0, 0, 0];
        public string Comment_BarIconMode { get; set; } = "Above BarIconMode values: 0= Horizontal Icons, 1= Vertical Icons, 2= Vertical Icons + Text, 3= Off. The arrays represent splitscreen screens, with first element being the default user.";
        public int[] BarTopLeftLocationX { get; set; } = [20, 20, 20, 20];
        public int[] BarTopLeftLocationY { get; set; } = [20, 20, 20, 20];
        public float[] BarScale { get; set; } = [1.0f, 1.0f, 1.0f, 1.0f];
        public int[] BarMaxIcons { get; set; } = [100, 100, 100, 100];
        public int[] BarMaxIconsPerRow { get; set; } = [20, 20, 20, 20];
        public int[] BarBackgroundMode { get; set; } = [0, 0, 0, 0];
        public string Comment_BarBackgroundMode { get; set; } = "Above BarBackgroundMode values: 0= Circles (behind each icon), 1= Rectangle (behind everything), 2= Off";
        public int[] BarBackgroundColorRGBA { get; set; } = [0, 0, 0, 128];
        public int[] BarTextColorRGBA { get; set; } = [255, 255, 255, 255];
        public bool[] BarShowBaitAndTackleInfo { get; set; } = [true, true, true, true];
        public int[] BarShowPercentagesMode { get; set; } = [0, 0, 0, 0];
        public int[] BarSortMode { get; set; } = [0, 0, 0, 0];
        public string Comment_BarSortMode { get; set; } = "Above BarSortMode values: 0= Sort Icons by Name (text mode only), 1= Sort icons by catch chance (Extra Check Frequency based), 2= Off";
        public bool[] BarExtraIconsAlwaysShow { get; set; } = [false, false, false, false];
        public bool[] BarExtraIconsMaxSize { get; set; } = [false, false, false, false];
        public bool[] BarExtraIconsBundles { get; set; } = [true, true, true, true];
        public bool[] BarExtraIconsAquarium { get; set; } = [true, true, true, true];
        public int[] BarUncaughtFishEffect { get; set; } = [0, 0, 0, 0];
        public int BarExtraCheckFrequency { get; set; } = 0;
        public int[] BarScanRadius { get; set; } = [20, 20, 20, 20];
        public bool[] BarCrabPotEnabled { get; set; } = [true, true, true, true];
        public int[] BarNonFishMode { get; set; } = [0, 0, 0, 0];
        public bool[] MinigamePreviewBar { get; set; } = [true, true, true, true];
        public bool[] MinigamePreviewRod { get; set; } = [true, true, true, true];
        public bool[] MinigamePreviewWater { get; set; } = [true, true, true, true];
        public bool[] MinigamePreviewSonar { get; set; } = [false, false, false, false];
    }
}
