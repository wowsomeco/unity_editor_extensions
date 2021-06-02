﻿using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  using EU = EditorUtils;

  #region Dropdown  

  public class Dropdown<T> where T : class {
    public bool Build(string lbl, T value, List<T> origins, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> onSelected = null) {
      int cur = origins.IndexOf(value);
      int selected = EditorGUILayout.Popup(lbl, cur, origins.Map(x => mapper(x)).ToArray());
      if (cur != selected) {
        onSelected.Invoke(new SelectState<T>(selected, origins[selected]));
        cur = selected;
      }

      return cur >= 0;
    }
  }

  #endregion

  #region Menu

  public class Menu<T> where T : class, new() {
    public class AddAction {
      public string Label;
      public Action<int> OnAdd;

      public AddAction(string lbl, Action<int> onAdd = null) {
        Label = lbl;
        OnAdd = onAdd;
      }
    }

    public class DeleteAction {
      public string Label;
      public Action<SelectState<T>> OnDelete;
      public Action<int> OnDeleted;

      public DeleteAction(Action<int> onDeleted = null, Action<SelectState<T>> onDel = null, string lbl = "X") {
        Label = lbl;
        OnDelete = onDel;
        OnDeleted = onDeleted;
      }
    }

    public class MoveAction {
      public string Label;
      public Action<int> Move;
      public Action OnMoved;
      public MoveAction(Action onMoved = null, Action<int> move = null, string lbl = "V") {
        OnMoved = onMoved;
        Move = move;
        Label = lbl;
      }
    }

    public class BuildCallback {
      public string Label;
      public T Value = null;
      public List<T> Origins;
      public ListExt.Mapper<T, string> Mapper;
      public Action<SelectState<T>> OnSelected = null;
      public AddAction AddAction = null;
      public DeleteAction DelAction = null;
      public MoveAction MoveAction = null;
      public Action<T> Prefix = null;
      public Action<T> Suffix = null;

      public BuildCallback(string lbl, List<T> or, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> os) {
        Label = lbl;
        Origins = or;
        Mapper = mapper;
        OnSelected = os;
      }

      public BuildCallback(string lbl, List<T> or, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> os, DeleteAction delAction)
      : this(lbl, or, mapper, os) {
        DelAction = delAction;
      }

      public BuildCallback(string lbl, T v, List<T> or, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> os, DeleteAction delAction)
      : this(lbl, or, mapper, os, delAction) {
        Value = v;
      }

      public BuildCallback(string lbl, T v, List<T> or, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> os)
      : this(lbl, or, mapper, os) {
        Value = v;
      }

      public BuildCallback(string lbl, T v, List<T> or, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> os, AddAction addAction, DeleteAction delAction)
      : this(lbl, v, or, mapper, os) {
        AddAction = addAction;
        DelAction = delAction;
      }

      public BuildCallback(string lbl, T v, List<T> or, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> os, AddAction addAction, DeleteAction delAction, MoveAction moveAction)
      : this(lbl, v, or, mapper, os, addAction, delAction) {
        MoveAction = moveAction;
      }

      public BuildCallback(string lbl, T v, List<T> or, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> os, AddAction addAction, DeleteAction delAction, Action<T> prefix)
      : this(lbl, v, or, mapper, os, addAction, delAction) {
        Prefix = prefix;
      }

      public BuildCallback(string lbl, T v, List<T> or, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> os, AddAction addAction, DeleteAction delAction, Action<T> prefix, Action<T> suffix)
      : this(lbl, v, or, mapper, os, addAction, delAction, prefix) {
        Suffix = suffix;
      }
    }

    public enum Align { V, H }

    Align m_alignment;

    public Menu(bool canMove = false, Align align = Align.V) {
      m_alignment = align;
    }

    public bool Build(BuildCallback cb) {
      int cur = cb.Origins.IndexOf(cb.Value);

      EU.VPadding(() => {
        if (null != cb.AddAction && GUILayout.Button(cb.AddAction.Label)) {
          T item = new T();
          cb.Origins.Add(item);
          if (null != cb.AddAction.OnAdd) cb.AddAction.OnAdd.Invoke(cb.Origins.Count);
        }

        EditorGUILayout.LabelField(cb.Label, EditorStyles.boldLabel);

        if (m_alignment == Align.V) { GUILayout.BeginVertical(); } else { GUILayout.BeginHorizontal(); }

        cb.Origins.Loop((t, idx) => {
          EU.HGroup(() => {
            bool isSelected = idx == cur;
            var style = new GUIStyle(GUI.skin.button);
            style.normal.textColor = isSelected ? Color.cyan : Color.white;

            if (null != cb.Prefix) cb.Prefix(t);

            if (GUILayout.Button(cb.Mapper(t), style)) {
              cb.OnSelected.Invoke(new SelectState<T>(idx, cb.Origins[idx]));
              cur = idx;
            }

            if (null != cb.Suffix) cb.Suffix(t);

            if (null != cb.MoveAction && cb.Origins.Count > idx + 1) {
              EU.BtnWithAlert("V", () => {
                if (cb.MoveAction.Move != null) { cb.MoveAction.Move(idx); } else { cb.Origins.Swap(idx, idx + 1); }
                cb.MoveAction.OnMoved.Invoke();
              }, GUILayout.Width(20f));
            }

            if (null != cb.DelAction) {
              EU.BtnWithAlert(cb.DelAction.Label, () => {
                if (null != cb.DelAction.OnDelete) { cb.DelAction.OnDelete(new SelectState<T>(idx, cb.Origins[idx])); } else { cb.Origins.RemoveAt(idx); }
                if (null != cb.DelAction.OnDeleted) cb.DelAction.OnDeleted.Invoke(idx);
              }, GUILayout.Width(20f));
            }
          });
        });

        if (m_alignment == Align.V) { GUILayout.EndVertical(); } else { GUILayout.EndHorizontal(); }
      });

      return cur > -1;
    }

    public bool Build(string lbl, T value, List<T> origins, ListExt.Mapper<T, string> mapper, Action<SelectState<T>> onSelected = null, AddAction addAction = null, DeleteAction delAction = null) {
      return Build(new BuildCallback(
        lbl,
        value,
        origins,
        mapper,
        onSelected,
        addAction,
        delAction
      ));
    }
  }

  #endregion

  #region AutoComplete

  public class AutoCompleteField {
    string m_value = string.Empty;
    Vector2 m_scrollPos;

    public void Build(string lbl, string value, List<string> selection, Action<string> onSelected) {
      EditorGUILayout.LabelField($"{lbl} : {value}", EditorStyles.boldLabel);

      GUI.SetNextControlName(lbl);
      m_value = EditorGUILayout.TextField("Search", m_value);

      if (m_value.IsEmpty() || GUI.GetNameOfFocusedControl() != lbl) return;

      List<string> foundItems = selection.FindAll(x => x.Contains(m_value));
      if (foundItems == null) return;

      GUILayout.BeginVertical("box");
      m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, GUILayout.Width(EditorGUIUtility.currentViewWidth - 50), GUILayout.Height(300));
      foundItems.ForEach(item => {
        EU.Btn(item, () => {
          m_value = item;
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