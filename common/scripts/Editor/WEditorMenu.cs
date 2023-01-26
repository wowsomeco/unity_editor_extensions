using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Wowsome {
  using EU = EditorUtils;

  public class EditorMenu<T> where T : class, new() {
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

    Align _alignment;

    public EditorMenu(bool canMove = false, Align align = Align.V) {
      _alignment = align;
    }

    public bool Build(BuildCallback cb) {
      int cur = cb.Origins.IndexOf(cb.Value);

      EU.VPadding(() => {
        if (null != cb.AddAction && GUILayout.Button(cb.AddAction.Label)) {
          T item = new T();
          cb.Origins.Add(item);

          cb.AddAction.OnAdd?.Invoke(cb.Origins.Count);
        }

        EditorGUILayout.LabelField(cb.Label, EditorStyles.boldLabel);

        if (_alignment == Align.V) { GUILayout.BeginVertical(); } else { GUILayout.BeginHorizontal(); }

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

                cb.MoveAction.OnMoved?.Invoke();
              }, GUILayout.Width(20f));
            }

            if (null != cb.DelAction) {
              EU.BtnWithAlert(cb.DelAction.Label, () => {
                if (null != cb.DelAction.OnDelete) { cb.DelAction.OnDelete(new SelectState<T>(idx, cb.Origins[idx])); } else { cb.Origins.RemoveAt(idx); }

                cb.DelAction.OnDeleted?.Invoke(idx);
              }, GUILayout.Width(20f));
            }
          });
        });

        if (_alignment == Align.V) { GUILayout.EndVertical(); } else { GUILayout.EndHorizontal(); }
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
}