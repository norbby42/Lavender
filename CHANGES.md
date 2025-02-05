# Upcoming
## Furniture
- added createdFurniture List
  - added GetFurnitureByTitel
  - added CreateShopFurniture (int amount, string furniture_titel)
## Item
- added customItemDatabase
  - added AddCustomItem
  - added AddCustomItemsFromJson
- added ItemCreator
## Recipe
- added customRecipeDatabase
  - added AddCustomRecipe
  - added AddCustomRecipesFromJson
- added RecipeCreator
## Developer Console
- added Command manager (GreenMushLib merge)
  - added PrintToDevConsole (GreenMushLib merge)
## Patches
- added ItemDatabase.DeSerialize Postfix
- added ItemOperations.SetCollectibleItemValues Postfix
- added Item.ItemAppearance.LoadSprite Prefix
- added Item.ItemAppearance.Loadprefab Prefix
- added RecipeDatabase.DeSerialize Postfix
- added Recipe.RecipeAppearance.LoadSprite Prefix
- added DeveloperConsole.HandleInput Prefix (GreenMushLib merge)

*Downgraded project to netstandard 2.0*
# v0.1.0 (December 8th 2024)
## Furniture
- added FurnitureCreator
  - added FurnitureConfigToFurniture
  - added Create
  - added CreateShopFurniture
- added Furniture Config
- added FurnitureAssetData
- added Furniture Handler -> Callback when Furniture gets loaded, which allows us to add custum scripts to the prefabs
- added Furniture Shop Restock Handler -> sell your furnitures!
## Runtime Importer
- added FastObjImporter
- added ImageLoader
## Patches
- added SavableScriptableObject.LoadFromPath Prefix
- added FurnitureShop.AddFurniture Prefix -> always skips remaining prefixes and original
- added FurnitureShop.Restock Prefix -> always skips remaining prefixes and original
- added BuildingSystem.AddFurniture Prefix -> always skips remaining prefixes and original
## Settings
- added DetailedLog
- added SceneLoadingDoneNotification
- added FurnitureShop_AddFurniture_Prefix_SkipOriginal
- added FurnitureShop_Restock_Prefix_SkipOriginal
- added BuildingSystem_AddFurniture_Prefix_SkipOriginal
