//////////////////////////////////////////////////////
// MegaSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JBooth.MicroSplat
{
#if __MICROSPLAT__ && (__MICROSPLAT_STREAMS__ || __MICROSPLAT_GLOBALTEXTURE__ || __MICROSPLAT_SNOW__ || __MICROSPLAT_SCATTER__ || __MICROSPLAT_PROCTEX__)
   public class TerrainPaintJob : ScriptableObject
   {
      public Terrain terrain;
      public Texture2D streamTex;
      public Texture2D tintTex;
      public Texture2D snowTex;
      public Texture2D scatterTex;
      public Texture2D biomeMask;
      public Texture2D biomeMask2;
      public Collider collider;

      public byte[] streamBuffer;
      public byte [] tintBuffer;
      public byte [] snowBuffer;
      public byte [] scatterBuffer;

      public byte [] biomeMaskBuffer;
      public byte [] biomeMaskBuffer2;

      public void RegisterUndo()
      {
         if (streamTex != null)
         {
            streamBuffer = streamTex.GetRawTextureData();
            UnityEditor.Undo.RegisterCompleteObjectUndo(this, "Terrain Edit");
         }
         if (tintTex != null)
         {
            tintBuffer = tintTex.GetRawTextureData ();
            UnityEditor.Undo.RegisterCompleteObjectUndo (this, "Terrain Edit");
         }
         if (snowTex != null)
         {
            snowBuffer = snowTex.GetRawTextureData ();
            UnityEditor.Undo.RegisterCompleteObjectUndo (this, "Terrain Edit");
         }
         if (scatterTex != null)
         {
            scatterBuffer = scatterTex.GetRawTextureData ();
            UnityEditor.Undo.RegisterCompleteObjectUndo (this, "Terrain Edit");
         }
         if (biomeMask != null)
         {
            biomeMaskBuffer = biomeMask.GetRawTextureData ();
            UnityEditor.Undo.RegisterCompleteObjectUndo (this, "Terrain Edit");
         }
         if (biomeMask2 != null)
         {
            biomeMaskBuffer2 = biomeMask2.GetRawTextureData ();
            UnityEditor.Undo.RegisterCompleteObjectUndo (this, "Terrain Edit");
         }
      }

      public void RestoreUndo()
      {
         if (streamBuffer != null && streamBuffer.Length > 0)
         {
            streamTex.LoadRawTextureData(streamBuffer);
            streamTex.Apply();
         }
         if (tintTex != null && tintBuffer.Length > 0)
         {
            tintTex.LoadRawTextureData (tintBuffer);
            tintTex.Apply ();
         }
         if (snowBuffer != null && snowBuffer.Length > 0)
         {
            snowTex.LoadRawTextureData (streamBuffer);
            snowTex.Apply ();
         }
         if (scatterBuffer != null && scatterBuffer.Length > 0)
         {
            scatterTex.LoadRawTextureData (scatterBuffer);
            scatterTex.Apply ();
         }

         if (biomeMask != null && biomeMaskBuffer.Length > 0)
         {
            biomeMask.LoadRawTextureData (biomeMaskBuffer);
            biomeMask.Apply ();
         }
         if (biomeMask2 != null && biomeMaskBuffer2.Length > 0)
         {
            biomeMask.LoadRawTextureData (biomeMaskBuffer2);
            biomeMask.Apply ();
         }
      }
   }
   #endif
}
