using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using ModdableInventory.Utils;

namespace ModdableInventory
{
    /// <summary>
    /// An abstract item, on which items of different types can be derived from.
    /// </summary>
    public abstract class Item
    {
        private const string SPRITES_FOLDER_PATH = "gamedata/sprites/items/";

        public string ItemStringID { get; private set; }
        /// <summary>
        /// Data parsed from an YAML file, which will define the item's properties.
        /// </summary>
        public Dictionary<string, string> ItemData { get; private set; }
        public string Name { get; private set; }
        public int Cost { get; private set; }
        public float Weight { get; private set; }
        /// <summary>
        /// How many of this item can be stacked into a single ItemSlot.
        /// </summary>
        public int StackLimit { get; private set; }
        /// <summary>
        /// Can this item be stacked into multiple ItemSlots?
        /// </summary>
        public bool MultiStack { get; private set; }

        public string SpritePath { get; private set; }
        public Sprite Sprite { get; private set; }

        public void Initialize(string itemStringID, Dictionary<string, string> itemData)
        {
            ItemStringID = itemStringID;
            ItemData = itemData;

            LoadProperties();
            Sprite = LoadSprite();
        }

        public virtual string PropertiesToString(int decimalPlaces = 2)
        {
            string text = "";

            text += $"cost: {Cost}\n";
            text += $"weight: {StringUtils.FloatToString(Weight, decimalPlaces)}\n";

            return text;
        }

        /// <summary>
        /// Extracts the Sprite of this Item to a subfolder in the game's install location.
        /// </summary>
        /// <remarks>
        /// (textures must be read/write enabled in the inspector to be able to extract)
        /// </remarks>
        public void ExtractItemSprite()
        {
            if (!EditorUtils.IsUnityEditor())
            {
                string fullPath = SPRITES_FOLDER_PATH + SpritePath;

                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                IOUtils.WriteFileToDirectory(fullPath, Sprite.texture.EncodeToPNG());
            }
        }

        protected virtual void LoadProperties()
        {
            Name = SetStringProperty("name", "generic_item");
            Cost = SetProperty<int>("cost", 0);
            Weight = SetProperty<float>("weight", 0);
            StackLimit = SetProperty<int>("stackLimit", 99);
            MultiStack = SetProperty<bool>("multiStack", true);
            SpritePath = SetStringProperty("spritePath", ItemStringID + ".png");

            if (Cost < 0) 
                throw new ArgumentOutOfRangeException($"cost of \"{Name}\"", "cannot be negative");
            if (StackLimit < 0) 
                throw new ArgumentOutOfRangeException($"stackLimit of \"{Name}\"", "cannot be negative");
        }

        protected string SetStringProperty(string key, string defaultValue)
        {
            string property;

            if (ItemData.ContainsKey(key))
            {
                property = ItemData[key];
            }
            else
            {
                property = defaultValue;
            }

            return property;
        }

        protected T SetProperty<T>(string key, T defaultValue) where T : struct
        {
            T property;

            if (ItemData.ContainsKey(key))
            {
                try
                {
                    property = (T) Convert.ChangeType(
                    ItemData[key], typeof(T), CultureInfo.InvariantCulture);
                }
                catch
                {
                    throw new FormatException(
                        $"value in key \"{key}\" of \"{Name}\"" 
                        + $" is not of type \"{typeof(T)}\""); 
                }
            }
            else
            {
                property = defaultValue;
            }

            return property;
        }

        private Sprite LoadSprite()
        {
            Sprite sprite = null;

            try { sprite = LoadExternalSprite(SPRITES_FOLDER_PATH, SpritePath); } catch {}
            if (sprite == null)
                sprite = LoadInternalSprite(SPRITES_FOLDER_PATH, Path.ChangeExtension(SpritePath, null));
            if (sprite == null)
                sprite = LoadInternalSprite(SPRITES_FOLDER_PATH, ItemStringID);
            if (sprite == null) 
                sprite = LoadInternalSprite(SPRITES_FOLDER_PATH, "generic_item");
            if (sprite == null)
                throw new NullReferenceException("No sprite found!");

            return sprite;
        }

        private Sprite LoadExternalSprite(string folderPath, string spritePath)
        {
            if (EditorUtils.IsUnityEditor())
            {
                return null;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(folderPath)); 

            byte[] byteArray = File.ReadAllBytes(folderPath + spritePath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(byteArray);
            
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height), 
                new Vector2(.5f, .5f), 100);

            return sprite;
        }

        private Sprite LoadInternalSprite(string folderPath, string spritePath)
        {
            return Resources.Load<Sprite>(folderPath + spritePath);            
        }
    }
}