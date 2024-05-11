using System;
using System.Collections.Generic;
using Sandbox;

namespace SpriteTools;

/// <summary>
/// A class that combines multiple textures into a single texture.
/// </summary>
public class TextureAtlas
{
    public int Size { get; }

    Texture Texture;
    int MaxFrameSize;
    static Dictionary<string, Texture> Cache = new();

    public TextureAtlas(List<string> texturePaths)
    {
        Size = (int)Math.Ceiling(Math.Sqrt(texturePaths.Count));
        if (Size % 2 != 0)
        {
            Size++;
        }

        var key = string.Join(",", texturePaths);
        if (Cache.TryGetValue(key, out var cachedTexture))
        {
            Texture = cachedTexture;
            MaxFrameSize = cachedTexture.Width / Size;
            Log.Info("TextureAtlas: Using cached texture");
            return;
        }

        List<Texture> textures = new();
        MaxFrameSize = 0;
        foreach (var path in texturePaths)
        {
            if (!FileSystem.Mounted.FileExists(path))
            {
                Log.Error($"TextureAtlas: Texture file not found: {path}");
                continue;
            }
            var texture = Texture.Load(FileSystem.Mounted, path);
            textures.Add(texture);
            MaxFrameSize = Math.Max(MaxFrameSize, Math.Max(texture.Width, texture.Height));
        }

        int x = 0;
        int y = 0;
        byte[] textureData = new byte[Size * Size * MaxFrameSize * MaxFrameSize * 4];
        foreach (var texture in textures)
        {
            if (x + texture.Width > Size * MaxFrameSize)
            {
                x = 0;
                y += MaxFrameSize;
            }
            if (y + texture.Height > Size * MaxFrameSize)
            {
                Log.Error("TextureAtlas: Texture too large for atlas");
                continue;
            }

            var pixels = texture.GetPixels();

            for (int i = 0; i < texture.Width; i++)
            {
                for (int j = 0; j < texture.Height; j++)
                {
                    var index = (x + i + (y + j) * Size * MaxFrameSize) * 4;
                    var textureIndex = i + j * texture.Width;
                    textureData[index] = pixels[textureIndex].r;
                    textureData[index + 1] = pixels[textureIndex].g;
                    textureData[index + 2] = pixels[textureIndex].b;
                    textureData[index + 3] = pixels[textureIndex].a;
                }
            }

            x += MaxFrameSize;
        }

        var builder = Texture.Create(Size * MaxFrameSize, Size * MaxFrameSize);
        builder.WithData(textureData);
        Texture = builder.Finish();

        Cache[key] = Texture;
    }

    public void Dispose()
    {
        Texture?.Dispose();
    }

    public Vector2 GetFrameTiling()
    {
        return new Vector2(1f / Size, 1f / Size);
    }

    public Vector2 GetFrameOffset(int index)
    {
        var x = index % Size;
        var y = index / Size;
        return new Vector2(x / (float)Size, y / (float)Size);
    }

    // Cast to texture
    public static implicit operator Texture(TextureAtlas atlas)
    {
        return atlas.Texture;
    }
}