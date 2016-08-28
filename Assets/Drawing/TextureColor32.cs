﻿using UnityEngine;

public class TextureColor32 : ManagedTexture<Color32>
{
    public class Pooler : ManagedPooler<Pooler, Color32>
    {
        public override ManagedTexture<Color32> CreateTexture(int width, int height)
        {
            return new TextureColor32(width, height);
        }
    }

    public static byte Lerp(byte a, byte b, byte u)
    {
        return (byte)(a + ((u * (b - a)) >> 8));
    }

    public static Color32 Lerp(Color32 a, Color32 b, byte u)
    {
        a.a = Lerp(a.a, b.a, u);
        a.r = Lerp(a.r, b.r, u);
        a.g = Lerp(a.g, b.g, u);
        a.b = Lerp(a.b, b.b, u);

        return a;
    }

    public static Blend<Color32> mask     = (canvas, brush) => brush.a > 0 ? brush : canvas;
    public static Blend<Color32> alpha    = (canvas, brush) => Lerp(canvas, brush, brush.a);
    public static Blend<Color32> replace  = (canvas, brush) => brush;

    public static Blend<Color32> stencilKeep = (canvas, brush) => Lerp(Color.clear, canvas, brush.a);
    public static Blend<Color32> stencilCut  = (canvas, brush) => Lerp(canvas, Color.clear, brush.a);

    public TextureColor32(int width, int height)
        : base(width, height, TextureFormat.ARGB32)
    {
    }

    public override void Apply()
    {
        if (dirty)
        {
            uTexture.SetPixels32(pixels);
            uTexture.Apply();
            dirty = false;
        }
    }
}
