#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  /// <summary>
  /// Create this script as a scriptable obj, then click Pack button on the Editor 
  /// to automagically pack all the sprites defined in the Data list.
  /// due to sprite atlas causes problem, right now it does not pack it into atlas, 
  /// but rather just set the texture settings on click Pack button according to the TextureType  
  /// </summary>
  [CreateAssetMenu(fileName = "SpriteResizer", menuName = "Wowsome/Utils/Sprite Resizer")]
  public class WSpriteResizer : ScriptableObject {
    // TODO: might want to add more type later, or at least make this customizable.
    [Serializable]
    public enum TextureType {
      Common,
      Background
    }

    /// <summary>
    /// The Sprite Packer Data
    /// It consists of the main path as well as one or more folders.    
    /// </summary>
    [Serializable]
    public class SpritePackerData {
      /// <summary>
      /// The main path 
      /// e.g. Assets/sprites
      /// </summary>
      public string path;
      /// <summary>
      /// You can define one or more folder path here
      /// </summary>
      public List<string> folders;
      public TextureType type;
      public TextureImporterFormat formatAndroid;
      public TextureImporterFormat formatIOS;
      public int minSize = 128;
      public int maxSize = 2048;

      public string FolderPaths {
        get {
          return folders.Fold(path, (prev, cur) => string.Format("{0}/{1}", prev, cur));
        }
      }
    }

    public List<SpritePackerData> data = new List<SpritePackerData>();
  }
}

#endif