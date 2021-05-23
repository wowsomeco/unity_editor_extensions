using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  using EU = EditorUtils;
  using TextureType = WSpriteImporter.TextureType;

  public static class TextureImporterExt {
    public static string PlatformAndroid() {
      // well done unity! keep changing everything!
#if UNITY_2018_1_OR_NEWER
      return "android";
#else
      return "Android";
#endif
    }

    public static string PlatformIos() {
      // well done unity! keep changing everything!
#if UNITY_2018_1_OR_NEWER
      return "ios";
#else
      return "iPhone";
#endif
    }

    public static bool GetOriginalImageSize(this TextureImporter importer, out int width, out int height) {
      if (importer != null) {
        object[] args = new object[2] { 0, 0 };
        MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        mi.Invoke(importer, args);

        width = (int)args[0];
        height = (int)args[1];

        return true;
      }

      height = width = 0;
      return false;
    }

    public static void OverridePlatformSettings(this TextureImporter importer, string platform, TextureImporterFormat format, int maxSize) {
      TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
      settings.overridden = true;
      settings.format = format;
      settings.maxTextureSize = maxSize;
      importer.SetPlatformTextureSettings(settings);
    }

    /// <summary>
    /// Post processor of texture of the background stuff.
    /// 1. ios, Set max size to 2048 format RGB 16 bit
    /// 2. android, Set max size to 1028 format RGB 16 bit
    /// </summary>    
    public static void SetBackgroundSettings(this TextureImporter importer) {
      // android
      importer.OverridePlatformSettings(PlatformAndroid(), TextureImporterFormat.RGB16, 1024);
      // ios
      importer.OverridePlatformSettings(PlatformIos(), TextureImporterFormat.RGB16, 2048);
      // apply changes
      EditorUtility.SetDirty(importer);
      importer.SaveAndReimport();
    }

    /// <summary>
    /// The Post Processor for the common images. How it works:
    /// 1. Get the original image dimensions e.g 400x200
    /// 2. Get the max size of width and height (in this case 400), then get previous power of two of it i.e 256
    /// 3. For android set max size to the POT value, set format to ASTC 8x8
    /// 4. For ios set max size to POT * 2, set format to ASTC 8x8
    /// </summary>    
    public static void SetCommonSettings(this TextureImporter importer, WSpriteImporter.SpritePackerData data) {
      int w, h;
      importer.GetOriginalImageSize(out w, out h);
      int prevPowerOfTwo = System.Math.Max(w, h).PrevPowerOfTwo();
      // android
      importer.OverridePlatformSettings(PlatformAndroid(), data.FormatAndroid, prevPowerOfTwo);
      // ios 
      importer.OverridePlatformSettings(PlatformIos(), data.FormatIOS, prevPowerOfTwo * 2);

      EditorUtility.SetDirty(importer);
      importer.SaveAndReimport();
    }
  }

  [CustomEditor(typeof(WSpriteImporter))]
  public class WSpriteImporterEditor : Editor {
    delegate void TexturePostProcessor(TextureImporter importer, WSpriteImporter.SpritePackerData data);

    Dictionary<TextureType, TexturePostProcessor> m_postProcessors = new Dictionary<TextureType, TexturePostProcessor>(){
      {TextureType.Common, (importer, data) => importer.SetCommonSettings(data)},
      {TextureType.Background, (importer, data)=> importer.SetBackgroundSettings()},
    };

    public override void OnInspectorGUI() {
      DrawDefaultInspector();
      WSpriteImporter tgt = (WSpriteImporter)target;

      EU.VPadding(() => {
        tgt.Data.ForEach(d => {
          if (GUILayout.Button("PACK " + d.FolderPaths.Ellipsis(20))) {
            d.Folders.ForEach(f => {
              string path = d.Path + "/" + f;
              string[] filePaths = Directory.GetFiles(path);
              foreach (string fName in filePaths) {
                if (fName.EndsWithMulti(new List<string> { "png", "jpg" })) {
                  TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(fName);
                  m_postProcessors[d.Type](textureImporter, d);
                }
              }
            });
          }

          EditorUtils.VSpacing();
        });
      });
    }
  }
}
