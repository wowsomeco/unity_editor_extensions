using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wowsome {
  public class FileBrowser {
    string m_lastPath = string.Empty;

    public FileBrowser(string defaultPath = "") {
      m_lastPath = defaultPath;
    }

    public void Build(string btnTxt, string acceptedFile, Action<string> onSelected) {
      if (GUILayout.Button(btnTxt)) {
        string path = EditorUtility.OpenFilePanel(btnTxt, string.IsNullOrEmpty(m_lastPath) ? "~/" : m_lastPath, acceptedFile);
        if (!string.IsNullOrEmpty(path)) {
          m_lastPath = path;
          onSelected(path);
        }
      }
    }
  }

  public class FolderBrowser {
    string m_lastPath = string.Empty;

    public void Build(string btnTxt, Action<string[]> onSelected) {
      if (GUILayout.Button(btnTxt)) {
        string path = EditorUtility.OpenFolderPanel(btnTxt, string.IsNullOrEmpty(m_lastPath) ? "~/" : m_lastPath, "");
        if (!string.IsNullOrEmpty(path)) {
          m_lastPath = path;
          string[] files = Directory.GetFiles(path);
          onSelected(files);
        }
      }
    }
  }
}