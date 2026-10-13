using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Aerolt.Enums;
using Aerolt.Helpers;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Aerolt.Classes
{

    public static class IconCache
    {
        private static readonly Dictionary<CharacterBody, Sprite> BodyIcons = new();
        private static readonly Dictionary<CharacterMaster, Sprite> MasterIcons = new();
        private static readonly Dictionary<SceneDef, Sprite> SceneIcons = new();

        public static bool Ready { get; private set; }

        public static void Warm(MonoBehaviour coroutineHost)
        {
            if (Ready) return;
            if (coroutineHost == null)
            {
                WarmImmediate();
                return;
            }

            coroutineHost.StartCoroutine(WarmRoutine());
        }

        private static void WarmImmediate()
        {
            foreach (var body in BodyCatalog.allBodyPrefabBodyBodyComponents) CacheBody(body);
            foreach (var master in MasterCatalog.allMasters) CacheMaster(master);
            foreach (var scene in SceneCatalog.allSceneDefs) CacheScene(scene);
            Ready = true;
        }

        private static IEnumerator WarmRoutine()
        {
            const int batchSize = 20;
            var count = 0;

            foreach (var body in BodyCatalog.allBodyPrefabBodyBodyComponents)
            {
                CacheBody(body);
                if (++count < batchSize) continue;
                count = 0;
                yield return null;
            }

            foreach (var master in MasterCatalog.allMasters)
            {
                CacheMaster(master);
                if (++count < batchSize) continue;
                count = 0;
                yield return null;
            }

            foreach (var scene in SceneCatalog.allSceneDefs)
            {
                CacheScene(scene);
                if (++count < batchSize) continue;
                count = 0;
                yield return null;
            }

            Ready = true;
            Tools.Log(LogLevel.Information,
                $"IconCache warmed: {BodyIcons.Count} bodies, {MasterIcons.Count} masters, {SceneIcons.Count} scenes");
        }

        private static void CacheBody(CharacterBody? body)
        {
            if (!body || BodyIcons.ContainsKey(body!)) return;
            var sprite = SpriteFromTexture(body!.portraitIcon);
            if (sprite) BodyIcons[body!] = sprite!;
        }

        private static void CacheMaster(CharacterMaster? master)
        {
            if (!master || MasterIcons.ContainsKey(master!)) return;
            if (!master!.bodyPrefab) return;
            var body = master.bodyPrefab.GetComponent<CharacterBody>();
            if (!body) return;
            var sprite = SpriteFromTexture(body.portraitIcon);
            if (sprite) MasterIcons[master] = sprite!;
        }

        private static void CacheScene(SceneDef? scene)
        {
            if (!scene || SceneIcons.ContainsKey(scene!)) return;
#pragma warning disable CS0618 // Type or member is obsolete
            var sprite = SpriteFromTexture(scene!.previewTexture);
#pragma warning restore CS0618 // Type or member is obsolete
            if (sprite) SceneIcons[scene!] = sprite!;
        }


        private static Sprite? SpriteFromTexture(Texture? texture)
        {
            if (texture is not Texture2D tex) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        private static readonly Dictionary<object, Sprite> ReferenceSprites = new();
        private static readonly FieldInfo? ItemIconReferenceField =
            typeof(ItemDef).GetField("pickupIconSpriteReference");

        private static readonly FieldInfo? EquipmentIconReferenceField =
            typeof(EquipmentDef).GetField("pickupIconSpriteReference");

        public static void LoadItemIcon(ItemDef? itemDef, Action<Sprite> onReady)
        {
            if (itemDef == null) return;
            if (itemDef.pickupIconSprite)
            {
                onReady(itemDef.pickupIconSprite);
                return;
            }

            LoadSpriteReference(ItemIconReferenceField?.GetValue(itemDef) as AssetReference, onReady);
        }

        public static void LoadEquipmentIcon(EquipmentDef? equipmentDef, Action<Sprite> onReady)
        {
            if (equipmentDef == null) return;
            if (equipmentDef.pickupIconSprite)
            {
                onReady(equipmentDef.pickupIconSprite);
                return;
            }

            LoadSpriteReference(EquipmentIconReferenceField?.GetValue(equipmentDef) as AssetReference, onReady);
        }

        private static void LoadSpriteReference(AssetReference? reference, Action<Sprite> onReady)
        {
            if (reference == null || !reference.RuntimeKeyIsValid()) return;

            var key = reference.RuntimeKey;
            if (ReferenceSprites.TryGetValue(key, out var cached))
            {
                if (cached) onReady(cached);
                return;
            }

            var handle = Addressables.LoadAssetAsync<Sprite>(key);
            handle.Completed += op =>
            {
                if (op.Status != AsyncOperationStatus.Succeeded || !op.Result) return;
                ReferenceSprites[key] = op.Result;
                onReady(op.Result);
            };
        }

        public static Sprite? GetBodyIcon(CharacterBody? body)
        {
            if (!body) return null;
            if (BodyIcons.TryGetValue(body!, out var sprite)) return sprite;
            CacheBody(body);
            return BodyIcons.TryGetValue(body!, out sprite) ? sprite : null;
        }

        public static Sprite? GetMasterIcon(CharacterMaster? master)
        {
            if (!master) return null;
            if (MasterIcons.TryGetValue(master!, out var sprite)) return sprite;
            CacheMaster(master);
            return MasterIcons.TryGetValue(master!, out sprite) ? sprite : null;
        }

        public static Sprite? GetSceneIcon(SceneDef? scene)
        {
            if (!scene) return null;
            if (SceneIcons.TryGetValue(scene!, out var sprite)) return sprite;
            CacheScene(scene);
            return SceneIcons.TryGetValue(scene!, out sprite) ? sprite : null;
        }
    }
}
