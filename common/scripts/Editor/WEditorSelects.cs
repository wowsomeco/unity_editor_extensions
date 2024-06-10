using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  using EU = EditorUtils;

  #region Dropdown  

  public class Dropdown<T> where T : class {
    public bool Build(string lbl, T value, List<T> origins, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> onSelected = null, params GUILayoutOption[] options) {
      int cur = origins.IndexOf(value);
      int selected = EditorGUILayout.Popup(lbl, cur, origins.Map(x => mapper(x)).ToArray(), options);
      if (cur != selected) {
        onSelected.Invoke(new SelectState<T>(selected, origins[selected]));
        cur = selected;
      }

      return cur >= 0;
    }
  }

  #endregion  

  #region AutoComplete

  public class AutoCompleteField {
    string _value = string.Empty;
    Vector2 _scrollPos;

    public void Build(string lbl, string value, List<string> selection, Action<string> onSelected) {
      EditorGUILayout.LabelField($"{lbl} : {value}", EditorStyles.boldLabel);

      GUI.SetNextControlName(lbl);
      _value = EditorGUILayout.TextField("Search", _value);

      if (_value.IsEmpty() || GUI.GetNameOfFocusedControl() != lbl) return;

      List<string> foundItems = selection.FindAll(x => x.Contains(_value));
      if (foundItems == null) return;

      GUILayout.BeginVertical("box");
      _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 50), GUILayout.Height(300));
      foundItems.ForEach(item => {
        EU.Btn(item, () => {
          _value = item;
          onSelected(item);
          GUI.FocusControl(null);
        });
      });
      EditorGUILayout.EndScrollView();
      GUILayout.EndVertical();
    }
  }

  #endregion
}