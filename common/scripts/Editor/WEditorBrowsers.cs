using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  public class FileBrowser {
    string _lastPath = string.Empty;

    public FileBrowser(string defaultPath = "") {
      _lastPath = defaultPath;
    }

    public void Build(string btnTxt, string acceptedFile, Action<string> onSelected) {
      if (GUILayout.Button(btnTxt)) {
        string path = EditorUtility.OpenFilePanel(btnTxt, string.IsNullOrEmpty(_lastPath) ? "~/" : _lastPath, acceptedFile);
        if (!string.IsNullOrEmpty(path)) {
          _lastPath = path;
          onSelected(path);
        }
      }
    }
  }

  public class FolderBrowser {
    string _lastPath = string.Empty;

    public void Build(string btnTxt, Action<string[]> onSelected) {
      if (GUILayout.Button(btnTxt)) {
        string path = EditorUtility.OpenFolderPanel(btnTxt, string.IsNullOrEmpty(_lastPath) ? "~/" : _lastPath, "");
        if (!string.IsNullOrEmpty(path)) {
          _lastPath = path;
          string[] files = Directory.GetFiles(path);
          onSelected(files);
        }
      }
    }
  }
}