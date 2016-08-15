﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DrawingTexture : ManagedTexture<Color>
{
    public DrawingTexture(int width, int height)
    {
        this.width = width;
        this.height = height;
        uTexture = Texture2DExtensions.Blank(width, height);

        pixels = new Color[width * height];

        dirty = true;
    }

    public DrawingTexture(Texture2D texture)
    {
        width = texture.width;
        height = texture.height;
        uTexture = texture;

        pixels = texture.GetPixels();
    }

    public override void Apply()
    {
        if (dirty)
        {
            uTexture.SetPixels(pixels);
            uTexture.Apply();
            dirty = false;
        }
    }
}

public struct DrawingBrush
{
    public static ManagedSprite<Color> Line(IntVector2 start,
                                             IntVector2 end,
                                             Color color,
                                             int thickness)
    {
        var tl = new IntVector2(Mathf.Min(start.x, end.x),
                                Mathf.Min(start.y, end.y));

        int left = Mathf.FloorToInt(thickness / 2f);

        IntVector2 size = new IntVector2(Mathf.Abs(end.x - start.x) + thickness,
                                         Mathf.Abs(end.y - start.y) + thickness);

        var pivot = tl * -1 + IntVector2.One * left;
        var rect = new Rect(0, 0, size.x, size.y);

        var dTexture = DrawingTexturePooler.Instance.GetTexture(size.x, size.y);
        var dSprite = new ManagedSprite<Color>(dTexture, rect, pivot);
        dSprite.Clear(Color.clear);

        var circle = DrawingTexturePooler.Instance.GetSprite(thickness, thickness, IntVector2.One * left);
        circle.Clear(Color.clear);
        Brush8.Circle<Color>(circle, thickness, color);
        {
            Blend<Color> alpha = (canvas, brush) => Blend.Lerp(canvas, brush, brush.a);

            Bresenham.PlotFunction plot = delegate (int x, int y)
            {
                dSprite.Blend(circle, alpha, brushPosition: new IntVector2(x, y));
            };

            Bresenham.Line(start.x, start.y, end.x, end.y, plot);
        }

        return dSprite;
    }
}

public class DrawingTexturePooler : ManagedPooler<DrawingTexturePooler, Color>
{
    public override ManagedTexture<Color> CreateTexture(int width, int height)
    {
        return new DrawingTexture(width, height);
    }
}

public class Texture8Pooler : ManagedPooler<Texture8Pooler, byte>
{
    public override ManagedTexture<byte> CreateTexture(int width, int height)
    {
        return new Texture8(width, height);
    }
}

public class ManagedPooler<TPooler, TPixel> : Singleton<TPooler>
    where TPooler : ManagedPooler<TPooler, TPixel>, new()
{
    private List<ManagedSprite<TPixel>> sprites = new List<ManagedSprite<TPixel>>();
    private List<ManagedTexture<TPixel>> textures = new List<ManagedTexture<TPixel>>();

    public virtual ManagedTexture<TPixel> CreateTexture(int width, int height)
    {
        throw new NotImplementedException();
    }

    public ManagedTexture<TPixel> GetTexture(int width, int height)
    {
        ManagedTexture<TPixel> dTexture;

        if (textures.Count > 0)
        {
            dTexture = textures[textures.Count - 1];
            textures.RemoveAt(textures.Count - 1);
        }
        else
        {
            dTexture = CreateTexture(256, 256);
        }

        return dTexture;
    }

    public ManagedSprite<TPixel> GetSprite(int width, int height, IntVector2 pivot = default(IntVector2))
    {
        var texture = GetTexture(width, height);

        ManagedSprite<TPixel> sprite;

        if (sprites.Count > 0)
        {
            sprite = sprites[sprites.Count - 1];
            sprites.RemoveAt(sprites.Count - 1);

            sprite.mTexture = texture;
            sprite.rect = new IntRect(0, 0, width, height);
            sprite.pivot = pivot;
        }
        else
        {
            sprite = new ManagedSprite<TPixel>(texture, new IntRect(0, 0, width, height), pivot);
        }

        return sprite;
    }

    public void FreeSprite(ManagedSprite<TPixel> sprite)
    {
        sprite.Dispose();
        sprites.Add(sprite);
    }

    public void FreeTexture(ManagedTexture<TPixel> texture)
    {
        textures.Add(texture);
    }
}

