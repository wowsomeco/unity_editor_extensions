using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wowsome {
  using EU = EditorUtils;

  public static class EditorUtils {
    public static void ApplyPrefab(this GameObject go) {
      GameObject prefab = (GameObject)PrefabUtility.InstantiatePrefab(go);
#if UNITY_2018_1_OR_NEWER
      PrefabUtility.ApplyPrefabInstance(prefab, InteractionMode.AutomatedAction);
#else
      GameObject instanceRoot = UnityEditor.PrefabUtility.FindRootGameObjectWithSameParentPrefab(prefab);
      UnityEngine.Object targetPrefab = UnityEditor.PrefabUtility.GetPrefabParent(instanceRoot);

      UnityEditor.PrefabUtility.ReplacePrefab(
        instanceRoot,
        targetPrefab,
        UnityEditor.ReplacePrefabOptions.ConnectToPrefab
      );
#endif
      AssetDatabase.SaveAssets();
      MonoBehaviour.DestroyImmediate(prefab.gameObject);

      UnityEditor.AssetDatabase.Refresh();
    }

    public static void ApplyPrefab(this Component c) {
      c.gameObject.ApplyPrefab();
    }

    public static void SetSceneDirty() {
      Scene curScene = SceneManager.GetActiveScene();
      EditorSceneManager.MarkSceneDirty(curScene);
      EditorSceneManager.SaveScene(curScene);
    }

    public static void Refresh() {
      UnityEditor.AssetDatabase.Refresh();
    }

    public static void Save() {
      UnityEditor.AssetDatabase.SaveAssets();
    }

    public static void SaveAndRefresh() {
      Save();
      Refresh();
    }

    public static void SaveScriptableObj(ScriptableObject obj) {
      EditorUtility.SetDirty(obj);
      SaveAndRefresh();
    }

    public static void Btn(string txt, Action onClick, params GUILayoutOption[] options) {
      if (GUILayout.Button(txt, options)) onClick();
    }

    public static void BtnWithAlert(string txt, Action onClick, params GUILayoutOption[] options) {
      if (GUILayout.Button(txt, options)) Alert(onClick);
    }

    public static void Alert(Action onYes, string content = "You Sure?", string title = "") {
      if (EditorUtility.DisplayDialog(title, content, "Yes", "No")) onYes();
    }

    public static void VSpacing(float pixels = 10f) {
      GUILayout.Space(pixels);
    }

    public static void VPadding(Action render, float pixels = 10f) {
      VSpacing(pixels);
      render();
      VSpacing(pixels);
    }

    public static void HGroup(Action render) {
      GUILayout.BeginHorizontal();
      render();
      GUILayout.EndHorizontal();
    }

    public static string TextfieldWithOk(string label, string value, Action<string> onSubmit, string submitLbl = "OK") {
      EU.HGroup(() => {
        value = EditorGUILayout.TextField(label, value);
        EU.Btn(submitLbl, () => onSubmit.Invoke(value));
      });
      return value;
    }

    public static Rect Resize(this Rect rect, Vector2 size) {
      Rect r = new Rect(rect);
      r.size = size;
      return r;
    }

    public static Rect ResizeWidth(this Rect rect, float w) {
      return rect.Resize(new Vector2(w, rect.size.y));
    }
  }

  #region Toggleable  

  public class Toggleable {
    public class Item {
      public string Text;
      public Action Build;

      public Item(string t, Action b) {
        Text = t;
        Build = b;
      }
    }

    public void Build(ToggleState state, List<Item> items, Action<ToggleState> onToggle) {
      EU.HGroup(() => {
        items.Loop((it, idx) => {
          bool selected = state.Idx == idx;
          var style = new GUIStyle(GUI.skin.button);
          style.normal.textColor = selected ? Color.blue : Color.black;
          if (GUILayout.Button(it.Text, style) && !selected) {
            onToggle(new ToggleState(!state.State));
          }
        });
      });

      Item item = items[state.Idx];
      EU.VPadding(() => item.Build());
    }
  }

  #endregion              
}
