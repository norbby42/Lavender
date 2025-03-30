# Upcoming
## Furniture
- fixed SavableScriptableObject.LoadFromPath being ambiguous in OS v0.4.05
- reworked createdFurniture List as FurnitureDatabase
  - renamed GetFurnitureByTitle to FetchFurnitureByTitle
  - added FetchFurnitureByID
  - added FurnitureDatabaseParent GameObject
- improved FurnitureCreator methods (Database usage & no duplicate loading)
- removed FurnitureHandler
- added FurniturePrefabHandlerAttribute
  - renamed all FurnitureHandlerAttribute functions to FurniturePrefabHandlerAttribute

## Storage
- added customStorageCategoryDatabase
  - added AddCustomStorageCategory
  - added AddCustomStorageCategoryFromJson
- added customStorageSpawnCategoryDatabase
  - added AddCustomStorageSpawnCategory
  - added AddCustomStorageSpawnCategoryFromJson
- added StorageCreator
  - added AddSimpleStorage

## Patches
- added StorageCategoryDatabase.DeSerialize Postfix
- added StorageCategoryDatabase.FetchCategoryByName Postfix
- added StorageSpawnCategoryDatabase.DeSerialize Postfix
- added StorageSpawnCategoryDatabase.FetchStorageSpawnCategory Postfix

# v0.2.1 (February 24th 2025)
- added UseBepinexLog
- added Harmony patching in a try/catch -> better error reporting
- fixed BuildingSystem.AddFurniture Prefix being ambiguous

# v0.2.0 (February 8th 2025)
## Furniture
- added createdFurniture List
  - added GetFurnitureByTitel
  - added CreateShopFurniture (int amount, string furniture_titel)
- updated FurnitureCreator
- updated FurnitureConfig
  - added Id
  - added new categories
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
