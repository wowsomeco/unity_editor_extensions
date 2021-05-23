#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  /// <summary>
  /// Attach this script to a gameobject prefab, then click Pack button on the Editor 
  /// to automagically pack all the sprites defined in the Data list.
  /// due to sprite atlas causes problem, right now it does not pack it into atlas, 
  /// but rather just set the texture settings on click Pack button according to the TextureType  
  /// </summary>
  public class WSpriteImporter : MonoBehaviour {
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
      public string Path;
      /// <summary>
      /// You can define one or more folder path here
      /// </summary>
      public List<string> Folders;
      public TextureType Type;
      public TextureImporterFormat FormatAndroid;
      public TextureImporterFormat FormatIOS;

      public string FolderPaths {
        get {
          return Folders.Fold(string.Empty, (prev, cur) => string.Format("{0}/{1}", prev, cur));
        }
      }
    }

    public List<SpritePackerData> Data = new List<SpritePackerData>();
  }
}

#endif