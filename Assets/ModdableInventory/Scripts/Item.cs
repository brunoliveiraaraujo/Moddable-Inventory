using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Utils;

namespace ModdableInventory
{
    public class Item
    {
        private const string SPRITES_FOLDER_PATH = "gamedata/sprites/items/";

        private Dictionary<string, string> itemData;

        public string IdName { get; private set; }
        public string Name { get; private set; }
        public int Cost { get; private set; }
        public float Weight { get; private set; }
        public int StackLimit { get; private set; }
        public bool MultiStack { get; private set; }

        public string SpritePath { get; private set; }
        public Sprite ItemSprite { get; private set; }

        public Dictionary<string, string> ItemData 
        { 
            get => itemData; private set => itemData = value; 
        }

        public virtual void Initialize(string idName, Dictionary<string, string> itemData)
        {
            this.IdName = idName;
            this.ItemData = itemData;

            Name = SetProperty("name", "generic_item");
            Cost = SetProperty<int>("cost", 0);
            Weight = SetProperty<float>("weight", 0);
            StackLimit = SetProperty<int>("stackLimit", 99);
            MultiStack = SetProperty<bool>("multiStack", true);
            SpritePath = SetProperty("spritePath", IdName);

            ItemSprite = LoadSprite(SpritePath);

            if (Cost < 0) 
                throw new ArgumentOutOfRangeException($"cost of \"{Name}\"", "cannot be negative");
            if (StackLimit < 0) 
                throw new ArgumentOutOfRangeException($"stackLimit of \"{Name}\"", "cannot be negative");
        }

        public virtual void LogItem(int decimalPlaces = 2)
        {
            // TODO: move this to a class responsible for UI stuff
            GameObject display = new GameObject(IdName);
            SpriteRenderer spriteRenderer = display.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = ItemSprite;
            display.transform.position = new Vector3(Weight/10, 1, -9);
        
            Debug.Log($"{Name}");
            Debug.Log($"    cost={Cost}");
            Debug.Log($"    stackLimit={StackLimit}");
            Debug.Log($"    multiStack={MultiStack}");
            Debug.Log($"    weight={StringUtils.FloatToString(Weight, decimalPlaces)}");
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

        private Sprite LoadSprite(string spritePath)
        {
            Sprite sprite = null;

            try { sprite = LoadGamedataSprite(SPRITES_FOLDER_PATH + spritePath); } catch {}
            if (sprite == null)
                sprite = Resources.Load<Sprite>(SPRITES_FOLDER_PATH + spritePath);
            if (sprite == null)
                sprite = Resources.Load<Sprite>(SPRITES_FOLDER_PATH + IdName);
            if (sprite == null) 
                sprite = Resources.Load<Sprite>(SPRITES_FOLDER_PATH + "generic_item");
            if (sprite == null)
                throw new NullReferenceException("No sprite found!");

            return sprite;
        }

        private Sprite LoadGamedataSprite(string spritePath)
        {
            #if UNITY_EDITOR
            #else
                Directory.CreateDirectory(SPRITES_FOLDER_PATH);
            #endif

            Sprite sprite = null;

            byte[] byteArray = File.ReadAllBytes(spritePath);
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