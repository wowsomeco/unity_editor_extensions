using System;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  using EU = EditorUtils;

  public class TextureUploader {
    [Serializable]
    public class UploadedModel {
      public Texture2D Texture { get; private set; }
      public string Filename { get; set; }
      public string FileExtension => Filename.LastSplit().LastSplit('.');
      public bool FilenameExists => !string.IsNullOrEmpty(Filename);
      public Vector2 TextureSize => new Vector2(Texture.width, Texture.height);
      public byte[] Encoded => FileExtension.Contains("png") ? Texture.EncodeToPNG() : Texture.EncodeToJPG();
      public string ContentType => FileExtension.Contains("png") ? "image/png" : "image/jpeg";

      public UploadedModel() { }

      public UploadedModel(Texture2D texture) {
        Texture = texture;
      }

      public UploadedModel(string filePath, string fileName) : this(filePath.ToTexture2D()) { Filename = fileName; }
    }

    string _lastUploadPath;
    UploadedModel _model = null;

    public void Build(string lbl, Action<UploadedModel> onUpload) {
      if (GUILayout.Button(lbl)) {
        string path = EditorUtility.OpenFilePanel(lbl, string.IsNullOrEmpty(_lastUploadPath) ? "~/" : _lastUploadPath, "png,jpg");
        if (path.Length != 0) {
          _lastUploadPath = path;
          _model = new UploadedModel(path, path.LastSplit());
        }
      }

      bool hasUploadedTexture = null != _model ? _model.Texture != null : false;

      EU.HGroup(() => {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (hasUploadedTexture && GUILayout.Button("X")) {
          _model = null;
        }
        EditorGUILayout.EndHorizontal();
      });

      if (hasUploadedTexture && null != _model) {
        EU.VSpacing();
        Rect r = GUILayoutUtility.GetLastRect();
        float windowWidth = EditorGUIUtility.currentViewWidth;
        r.size = _model.TextureSize.AspectRatio(windowWidth - 30f);
        EditorGUI.DrawPreviewTexture(r, _model.Texture);
        EU.VSpacing(r.height);

        EditorGUILayout.LabelField(_lastUploadPath, EditorStyles.miniBoldLabel);
        EU.VPadding(() => {
          _model.Filename = EditorGUILayout.TextField("Filename", _model.Filename);
        });

        EU.VPadding(() => {
          if (_model.FilenameExists && GUILayout.Button("UPLOAD")) onUpload(_model);
        });
      }
    }
  }
}