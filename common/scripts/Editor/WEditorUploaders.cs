using System;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  using EU = EditorUtils;

  public class TextureUploader {
    [Serializable]
    public class UploadedModel {
      public Texture2D Texture;
      public string Filename;

      public string FileExtension {
        get { return Filename.LastSplit().LastSplit('.'); }
      }

      public bool FilenameExists {
        get { return !string.IsNullOrEmpty(Filename); }
      }

      public Vector2 TextureSize {
        get { return new Vector2(Texture.width, Texture.height); }
      }

      public byte[] Encoded {
        get { return FileExtension.Contains("png") ? Texture.EncodeToPNG() : Texture.EncodeToJPG(); }
      }

      public UploadedModel(Texture2D texture) {
        Texture = texture;
      }

      public UploadedModel(string filePath, string fileName) : this(filePath.ToTexture2D()) { Filename = fileName; }
    }

    string m_lastUploadPath;
    UploadedModel m_model = null;

    public void Build(string lbl, Action<UploadedModel> onUpload) {
      if (GUILayout.Button(lbl)) {
        string path = EditorUtility.OpenFilePanel(lbl, string.IsNullOrEmpty(m_lastUploadPath) ? "~/" : m_lastUploadPath, "png,jpg");
        if (path.Length != 0) {
          m_lastUploadPath = path;
          m_model = new UploadedModel(path, path.LastSplit());
        }
      }

      bool hasUploadedTexture = null != m_model ? m_model.Texture != null : false;

      EU.HGroup(() => {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (hasUploadedTexture && GUILayout.Button("X")) {
          m_model = null;
        }
        EditorGUILayout.EndHorizontal();
      });

      if (hasUploadedTexture && null != m_model) {
        EU.VSpacing();
        Rect r = GUILayoutUtility.GetLastRect();
        float windowWidth = EditorGUIUtility.currentViewWidth;
        r.size = m_model.TextureSize.AspectRatio(windowWidth - 30f);
        EditorGUI.DrawPreviewTexture(r, m_model.Texture);
        EU.VSpacing(r.height);

        EditorGUILayout.LabelField(m_lastUploadPath, EditorStyles.miniBoldLabel);
        EU.VPadding(() => {
          m_model.Filename = EditorGUILayout.TextField("Filename", m_model.Filename);
        });

        EU.VPadding(() => {
          if (m_model.FilenameExists && GUILayout.Button("UPLOAD")) onUpload(m_model);
        });
      }
    }
  }
}