namespace BgControls.Windows.DragDrop;

/// <summary>
/// 定义拖放效果呈现器的接口.
/// </summary>
public interface IEffectsPresenter
{
    /// <summary>
    /// Gets or sets 拖放操作的效果.
    /// </summary>
    DragDropEffects Effects { get; set; }
}