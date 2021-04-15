# Moddable-Inventory

An easy-to-use and easy-to-mod inventory system for Unity.

    NOTE: this software was made using Unity 2020.3, and may not be compatible with other versions.

---

## How to install

To install Moddable-Inventory in your Unity project, just copy the "./Assets/ModdableInventory/" folder into your project's "Assets" folder.

By also copying "./Assets/Demo/" into your project's "Assets" folder, you can play a Demo that shows an example of how Moddable-Inventory can be used.

## Demo

Once Moddable-Inventory is installed into your project, you can run the Demo scene available at "./Assets/Demo/Scenes/InventoryDemo.unity".

In the Demo scene you can browse an inventory full of items, filter by category, and equip/unequip items.
It also includes a tooltip system that shows the data related to that item.

By looking at how the Demo scene is setup, and the files included with it, you can see how Moddable-Inventory can be used in your project.

## Instructions (developer)

Moddable-Inventory can be used via 3 MonoBehaviour scripts:

* ItemDatabase which holds a catalogue of all items in the game;
* Inventory which holds data about the items in the player's inventory, the player's money and carry capacity;
* and Equipment which defines the equipment slots on which different types of items can be equipped.

The data in these 3 scripts are read from easy-to-mod YAML files. In the Demo, inside the Unity Editor they are located in "./Assets/Demo/Resources/gamedata/config/" and are named "itemDatbase.yaml", "inventory.yaml" and "equipment.yaml".

The image/sprite data for the items used in the Demo is included in "./Assets/Demo/Resources/gamedata/sprites/items/"

NOTE: for the item sprites to be able to be extracted and used by a modder without causing errors, all item's textures must have the following settings in the inspector:

* Texture Type: Sprite (2D and UI)
* Read/Write Enabled: true
* Compression: None

To add a new type of item in your game you just need to add a new Class that extends ModdableInventory.Item.

Examples of different types of item can be found in the Demo at "./Assets/Demo/Scripts/ItemTypes/".

    For example the "Armor" class extends "Item" and adds a new property exclusive to Armors: "Defense". Then it overrides the LoadProperties() function found in Item, to also load the new Defense property from the item's "defense" value in itemDatabase.yaml (using 0 as a default); then it also overrides the PropertiesToString(int decimalPlaces = 2) method, to add the Defense property to the item's text displayed in the tooltip in the Demo.

To customize what items are in the game and the settings for inventory and equipment, check the instructions in each respective YAML file.

## Instructions (modder)

With Moddable-Inventory a modder can easily modify/add/remove items, change inventory settings, and what equipment slots are available in any game that uses this system.

Once a build of the game is run for the first time (i.e. the "./gamedata/" folder does not exist); it creates copies of it's internal YAML files, to "./gamedata/config/" (path relative to the game's executable). There the modder can easily mod the items, inventory and equipment slots of the game, by following the instructions in these YAML files. The modder can also add sprites for new items in "./gamedata/sprites/items/".

If the modder changes the "extractSpriteData" setting in "itemDatabase.yaml" to true, and runs the game. All internal item sprites are extracted to "./gamedata/sprites/items/" and can then also be modified by the modder.
