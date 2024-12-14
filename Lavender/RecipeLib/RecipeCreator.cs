using Lavender.RuntimeImporter;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Lavender.RecipeLib
{
    internal class RecipeCreator
    {
        private static string ExtractString(string input)
        {
            // Define a regex pattern to match string_two
            string pattern = @"#AB<(.*?)>$";
            var match = Regex.Match(input, pattern);

            if (match.Success)
            {
                // Extract and return string_two from the match group
                return match.Groups[1].Value;
            }
            else
            {
                throw new ArgumentException("Input string is not in the expected format.");
            }
        }

        public static Sprite RecipeSpriteFromAssetBundle(string data_path)
        {
            string path = data_path.Substring(0, data_path.IndexOf("#"));

            try
            {
                string sprite_name = ExtractString(data_path);

                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var assetBundle = AssetBundle.LoadFromStream(fileStream);
                if (assetBundle == null)
                {
                    LavenderLog.Error($"Error while loading Recipe Sprite: couldn't get AssetBundle at '{path}'!");
                    return null;
                }

                var sprite = assetBundle.LoadAsset<Sprite>(sprite_name);

                assetBundle.Unload(false);
                return sprite;
            }
            catch (Exception e)
            {
                LavenderLog.Error($"RecipeSpriteFromAssetBundle(): '{e}'");
                return null;
            }
        }
    }
}
