﻿using HarmonyLib;
using Lavender.RuntimeImporter;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Lavender.RecipeLib
{
    public static class RecipePatches
    {
        [HarmonyPatch(typeof(RecipeDatabase), nameof(RecipeDatabase.DeSerialize))]
        [HarmonyPostfix]
        static void RecipeDatabase_DeSerialize_Postfix(ref object __result, Type type, string serializedState)
        {
            RecipeDatabase.RecipesHeader result = (RecipeDatabase.RecipesHeader)__result;
            if (type == typeof(RecipeDatabase.RecipesHeader))
            {
                List<Recipe>? vanilla_db = result.Recipes as List<Recipe>;
                if (vanilla_db != null)
                {
                    List<Recipe> db = new List<Recipe>();
                    db.AddRange(vanilla_db);

                    List<int> recipe_ids = new List<int>();
                    foreach (Recipe recipe in vanilla_db)
                    {
                        recipe_ids.Add(recipe.ID);
                    }

                    foreach (Recipe recipe in Lavender.customRecipeDatabase)
                    {
                        if (recipe_ids.Contains(recipe.ID))
                        {
                            LavenderLog.Log($"Replacing vanilla Recipe id={recipe.ID} with mod Recipe!");
                            db.Remove(db.Find((Recipe i) => i.ID == recipe.ID));
                            db.Add(recipe);
                        }
                        else
                        {
                            db.Add(recipe);
                        }
                    }

                    result.Recipes = db;
                    __result = result;

                    LavenderLog.Log($"Successfully added {Lavender.customRecipeDatabase.Count} Mod Recipes");
                }
                else
                {
                    LavenderLog.Error("RecipeDatabase.DeSerialize: This shouldn't happen!");
                }
            }

            return;
        }

        [HarmonyPatch(typeof(Recipe.RecipeAppearance), nameof(Recipe.RecipeAppearance.LoadSprite))]
        [HarmonyPrefix]
        static bool Recipe_RecipeAppearance_LoadSprite_Prefix(Recipe.RecipeAppearance __instance)
        {
            if (!string.IsNullOrEmpty(__instance.SpritePath))
            {
                if (__instance.SpritePath.StartsWith("Lavender_SRC#"))
                {
                    string spritePath = __instance.SpritePath.Substring("Lavender_SRC#".Length);

                    __instance.Sprite = ImageLoader.LoadSprite(spritePath);

                    return false;
                }
                else if (__instance.SpritePath.StartsWith("Lavender_AB#"))
                {
                    string spritePath = __instance.SpritePath.Substring("Lavender_AB#".Length);

                    __instance.Sprite = RecipeCreator.RecipeSpriteFromAssetBundle(spritePath);

                    return false;
                }
            }
            return true;
        }
    }
}
