namespace Wowsome {
  #region Toggleable

  public class ToggleState {
    public bool State { get; set; }
    public int Idx {
      get { return State ? 1 : 0; }
    }

    public ToggleState(bool state = false) {
      State = state;
    }

    public void Toggle() {
      State = !State;
    }
  }

  #endregion

  #region Selectable

  public class SelectState<T> where T : class {
    public int Idx { get; set; }
    public T Model { get; set; }

    public bool Selected {
      get { return Idx > -1; }
    }

    public SelectState() : this(-1, null) { }

    public SelectState(int idx, T model) {
      Idx = idx;
      Model = model;
    }

    public void Reset() {
      Idx = -1;
      Model = null;
    }
  }

  #endregion
}
