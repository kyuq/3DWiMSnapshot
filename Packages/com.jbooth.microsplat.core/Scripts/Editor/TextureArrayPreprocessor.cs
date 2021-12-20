//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace JBooth.MicroSplat
{
   class TextureArrayPreProcessor : AssetPostprocessor
   {
      // this is a shitty hash, but good enough for unity versions..
      static int HashString(string str)
      {
         unchecked
         {
            int h = 0;
            int[] hashPrimes = { 3, 5, 7, 11, 13, 17, 23, 27 };
            int pidx = 0;
            foreach (char c in str)
            {
               h += (int)c * hashPrimes[pidx % hashPrimes.Length];
               pidx++;
            }
            return h;
         }
         
      }

      static int GetNewHash(TextureArrayConfig cfg)
      {
         unchecked
         {
            var settings = TextureArrayConfigEditor.GetSettingsGroup(cfg, UnityEditor.EditorUserBuildSettings.activeBuildTarget);
            int h = 17;

            h = h * (int)TextureArrayConfigEditor.GetTextureFormat(cfg, settings.diffuseSettings.compression, settings.diffuseSettings.compressionQuality) * 7;
            h = h * (int)TextureArrayConfigEditor.GetTextureFormat(cfg, settings.normalSettings.compression, settings.normalSettings.compressionQuality) * 13;
            h = h * (int)TextureArrayConfigEditor.GetTextureFormat(cfg, settings.emissiveSettings.compression, settings.emissiveSettings.compressionQuality) * 17;
            h = h * (int)TextureArrayConfigEditor.GetTextureFormat(cfg, settings.antiTileSettings.compression, settings.antiTileSettings.compressionQuality) * 31;
            h = h * (int)TextureArrayConfigEditor.GetTextureFormat(cfg, settings.smoothSettings.compression, settings.smoothSettings.compressionQuality) * 37;
            h = h * HashString(Application.unityVersion) * 43;
            //h = h * EditorUserBuildSettings.activeBuildTarget.GetHashCode () * 47;
            return h;
         }
      }

      public static bool sIsPostProcessing = false;

      static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
      {
         var updates = new HashSet<TextureArrayConfig>();
         AddChangedConfigsToHashSet(updates, importedAssets);
         AddChangedConfigsToHashSet(updates, movedAssets);
         AddChangedConfigsToHashSet(updates, movedFromAssetPaths);

         foreach (var updatedConfig in updates)
         {
            CheckConfigForUpdates(updatedConfig);
         }
      }

      private static void AddChangedConfigsToHashSet(HashSet<TextureArrayConfig> hashSet, string[] paths)
      {
         for (int i = 0; i < paths.Length; i++)
         {
            var cfg = AssetDatabase.LoadAssetAtPath<TextureArrayConfig>(paths[i]);
            if (cfg != null)
            {
               hashSet.Add(cfg);
            }
         }
      }

      private static void CheckConfigForUpdates(TextureArrayConfig cfg)
      {
         int hash = GetNewHash(cfg);
         if (hash != cfg.hash)
         {
            cfg.hash = hash;
            EditorUtility.SetDirty(cfg);
            try 
            { 
               sIsPostProcessing = true;
               TextureArrayConfigEditor.CompileConfig(cfg);
            }
            finally
            {
               sIsPostProcessing = false;
               AssetDatabase.Refresh();
               AssetDatabase.SaveAssets();
               MicroSplatTerrain.SyncAll();
            }
         }
      }
   }
}
