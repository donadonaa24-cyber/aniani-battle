namespace AniBato.SpriteForge
{
    public sealed class Manifest
    {
        public string character, sourceDirectory, outputDirectory, contactPath;
        public int targetHeight = 112, cellWidth = 224, cellHeight = 160, baseline = 159, paletteSize = 32;
        public int keyTolerance = 90;
        public float pixelsPerUnit = 50;
        public int[] anchorRgbMin = new[] { 115, 45, 0 }, anchorRgbMax = new[] { 255, 180, 85 };
        public double anchorRedGreenRatio = 1.5;
        public Animation[] animations;
    }
    public sealed class Animation
    {
        public string name, anchor = "feet";
        public int frames, fps = 10;
        public int inbetweenFrames;
        public bool stableIdle;
        public string sourceClip;
        public string prebuiltSheet; // Workspace-relative, already aligned fixed-grid frames. Raw scale reference is unchanged.
        public int[] sourceFrames;
        public double sourceScaleFactor = 1;
        public int headOffset;
        public bool loop, validateHeight;
        public double[] sourceAnchors; // Optional anatomical root positions, pixels relative to each detected frame.
        public Binding[] bindings;
    }
    public sealed class Binding { public int State, ActiveFrame; public string Clip; }
    public sealed class Metadata
    {
        public int schemaVersion = 1;
        public string name;
        public int frameCount, cellWidth, cellHeight, fps, targetHeight, baseline;
        public bool loop;
        public float pivotX = 0.5f, pivotY, pixelsPerUnit;
        public Binding[] bindings;
        public int[] headHeights;
    }
}


