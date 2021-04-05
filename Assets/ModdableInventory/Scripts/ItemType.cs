using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using ModdableInventory.Utils;

namespace ModdableInventory
{
    public abstract class ItemType
    {
        private const string SPRITES_FOLDER_PATH = "gamedata/sprites/items/";

        private Dictionary<string, string> itemData;

        public string IDName { get; private set; }
        public string Name { get; private set; }
        public int Cost { get; private set; }
        public float Weight { get; private set; }
        public int StackLimit { get; private set; }
        public bool MultiStack { get; private set; }

        public string SpritePath { get; private set; }
        public Sprite Sprite { get; private set; }

        public Dictionary<string, string> ItemData 
        { 
            get => itemData; private set => itemData = value; 
        }

        public void Initialize(string idName, Dictionary<string, string> itemData)
        {
            this.IDName = idName;
            this.ItemData = itemData;

            LoadProperties();

            Sprite = LoadSprite();

            if (Cost < 0) 
                throw new ArgumentOutOfRangeException($"cost of \"{Name}\"", "cannot be negative");
            if (StackLimit < 0) 
                throw new ArgumentOutOfRangeException($"stackLimit of \"{Name}\"", "cannot be negative");
        }

        public virtual string PropertiesToString(int decimalPlaces = 2)
        {
            string text = "";

            text += $"cost: {Cost}\n";
            text += $"weight: {StringUtils.FloatToString(Weight, decimalPlaces)}\n";

            return text;
        }

        // Textures must be read/write enabled in the inspector to be able to extract
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
            Name = SetProperty("name", "generic_item");
            Cost = SetProperty<int>("cost", 0);
            Weight = SetProperty<float>("weight", 0);
            StackLimit = SetProperty<int>("stackLimit", 99);
            MultiStack = SetProperty<bool>("multiStack", true);
            SpritePath = SetProperty("spritePath", IDName + ".png");
        }

        protected string SetProperty(string key, string defaultValue)
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

            try { sprite = LoadGamedataSprite((SPRITES_FOLDER_PATH + SpritePath)); } catch {}
            if (sprite == null)
                sprite = Resources.Load<Sprite>(SPRITES_FOLDER_PATH + Path.ChangeExtension(SpritePath, null));
            if (sprite == null)
                sprite = Resources.Load<Sprite>(SPRITES_FOLDER_PATH + IDName);
            if (sprite == null) 
                sprite = Resources.Load<Sprite>(SPRITES_FOLDER_PATH + "generic_item");
            if (sprite == null)
                throw new NullReferenceException("No sprite found!");

            return sprite;
        }

        private Sprite LoadGamedataSprite(string fullPath)
        {
            if (!EditorUtils.IsUnityEditor())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)); 
            }

            Sprite sprite = null;

            byte[] byteArray = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(byteArray);
            
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height), 
                new Vector2(.5f, .5f), 100);

            return sprite;
        }
    }
}