using Lavender.RuntimeImporter;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Lavender.ItemLib
{
    public class ItemCreator
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

        public static Sprite ItemSpriteFromAssetBundle(string data_path)
        {
            string path = data_path.Substring(0, data_path.IndexOf("#"));

            try
            {
                string sprite_name = ExtractString(data_path);

                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var assetBundle = AssetBundle.LoadFromStream(fileStream);
                if (assetBundle == null)
                {
                    LavenderLog.Error($"Error while loading Item Sprite: couldn't get AssetBundle at '{path}'!");
                    return null;
                }

                var sprite = assetBundle.LoadAsset<Sprite>(sprite_name);

                assetBundle.Unload(false);
                return sprite;
            }
            catch(Exception e)
            {
                LavenderLog.Error($"ItemSpriteFromAssetBundle(): '{e}'");
                return null;
            }
        }

        public static GameObject ItemPrefabFromAssetBundle(string data_path)
        {
            string path = data_path.Substring(0, data_path.IndexOf("#"));

            try
            {
                string prefab_name = ExtractString(data_path);

                var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                var assetBundle = AssetBundle.LoadFromStream(fileStream);
                if (assetBundle == null)
                {
                    LavenderLog.Error($"Error while loading Item Prefab: couldn't get AssetBundle at '{path}'!");
                    return null;
                }

                var prefab = assetBundle.LoadAsset<GameObject>(prefab_name);

                prefab.AddComponent<CollectibleItem>();
                prefab.layer = 17;

                assetBundle.Unload(false);
                return prefab;
            }
            catch (Exception e)
            {
                LavenderLog.Error($"ItemPrefabFromAssetBundle(): '{e}'");
                return null;
            }
        }

        public static GameObject ItemPrefabFromOBJ(string meshPath, string texturePath, string name)
        {
            GameObject obj = new GameObject(name);

            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));

            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();

            try
            {
                Mesh mesh = FastObjImporter.Instance.ImportFile(meshPath);
                mesh.name = name + "_Mesh";
                meshFilter.mesh = mesh;

                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(texturePath));
                mat.mainTexture = tex;
                meshRenderer.material = mat;
            }
            catch(Exception e)
            {
                LavenderLog.Error($"ItemPrefabFromOBJ(): {e}");

                return null;
            }

            obj.AddComponent<MeshCollider>();
            obj.AddComponent<CollectibleItem>();
            obj.layer = 17;

            return obj;
        }
    }
}
