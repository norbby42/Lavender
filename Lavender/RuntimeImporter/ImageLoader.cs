using System;
using System.IO;
using UnityEngine;

namespace Lavender.RuntimeImporter
{
    public static class ImageLoader
    {
        public static Texture2D? LoadImage(string path)
        {
            if(File.Exists(path))
            {
                try
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(File.ReadAllBytes(path));
                    return tex;
                }
                catch(Exception e)
                {
                    LavenderLog.Error($"ImageLoader: {e}");
                    return null;
                }
            }
            else
            {
                LavenderLog.Error($"ImageLoader: Couldn't find image at '{path}'!");
                return null;
            }
        }

        public static Sprite? LoadSprite(string path)
        {
            if(File.Exists(path))
            {
                try
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(File.ReadAllBytes(path));

                    Sprite sprite = Sprite.Create(tex, new Rect(.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    return sprite;
                }
                catch (Exception e)
                {
                    LavenderLog.Error($"ImageLoader: {e}");
                    return null;
                }
            }
            else
            {
                LavenderLog.Error($"ImageLoader: Couldn't find image at '{path}'!");
                return null;
            }
        }
    }
}
