namespace BgControls.Windows.Input.Touch;

/// <summary>
/// 手势冲突管理器，用于协调同一元素上多个手势的激活与停用逻辑.
/// </summary>
internal class GestureConflictManager
{
    /// <summary>
    /// 手势信息元组，用于存储手势的名称、结束回调和令牌.
    /// </summary>
    private class GestureTuple
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GestureTuple"/> class.
        /// </summary>
        /// <param name="gestureName">手势名称.</param>
        /// <param name="forcedFinish">强制结束回调.</param>
        /// <param name="token">停用令牌.</param>
        public GestureTuple(string gestureName, Action forcedFinish, GestureDeactivationToken token)
        {
            // 初始化字段.
            this.GestureName = gestureName;
            this.ForcedFinish = forcedFinish;
            this.Token = token;
        }

        /// <summary>
        /// Gets 手势名称.
        /// </summary>
        public string GestureName { get; }

        /// <summary>
        /// Gets 强制结束手势的回调动作.
        /// </summary>
        public Action ForcedFinish { get; }

        /// <summary>
        /// Gets 手势停用令牌.
        /// </summary>
        public GestureDeactivationToken Token { get; }

        public override bool Equals(object? obj)
        {
            return obj is GestureTuple tuple &&
                   EqualityComparer<Action>.Default.Equals(ForcedFinish, tuple.ForcedFinish);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// 内部存储的当前活跃手势元组.
    /// </summary>
    private GestureTuple? active;

    /// <summary>
    /// Gets or sets 当前活跃的手势元组.
    /// </summary>
    private GestureTuple? Active
    {
        get
        {
            return this.active;
        }

        set
        {
            if (this.active != value)
            {
                this.active = value;

                // 状态变更时触发事件.
                this.OnActiveChanged();
            }
        }
    }

    /// <summary>
    /// 当活跃手势发生改变时触发的事件.
    /// </summary>
    public event EventHandler? ActiveGestureChanged;

    /// <summary>
    /// 获取当前活跃的手势名称.
    /// </summary>
    /// <returns>手势名称，若无活跃手势则返回 null.</returns>
    public string? GetActiveGestureName()
    {
        if (this.Active != null)
        {
            return this.Active.GestureName;
        }

        return null;
    }

    /// <summary>
    /// Gets a value indicating whether 当前是否存在活跃的手势.
    /// </summary>
    /// <returns>是否存在活跃的手势.</returns>
    public bool HasActiveGesture()
    {
        return this.Active != null;
    }

    /// <summary>
    /// 判断候选手势是否可以激活.
    /// </summary>
    /// <param name="candidate">候选手势名称.</param>
    /// <returns>如果可以激活则返回 true; 否则返回 false.</returns>
    public bool CanActivateGesture(string candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate, nameof(candidate));

        // 如果没有活跃手势，或者手势管理器允许当前手势转换到候选手势.
        if (this.Active == null || GestureManager.IsGestureTransitionAllowed(this.Active.GestureName, candidate))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 激活指定的手势.
    /// </summary>
    /// <param name="element">关联的 UI 元素.</param>
    /// <param name="gestureName">手势名称.</param>
    /// <param name="forcedFinish">强制结束回调.</param>
    /// <returns>手势停用令牌.</returns>
    public GestureDeactivationToken ActivateGesture(UIElement element, string gestureName, Action forcedFinish)
    {
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        ArgumentNullException.ThrowIfNull(gestureName, nameof(gestureName));
        ArgumentNullException.ThrowIfNull(forcedFinish, nameof(forcedFinish));

        // 验证手势是否允许激活.
        if (!this.CanActivateGesture(gestureName))
        {
            throw new InvalidOperationException("Could not activate gesture.");
        }

        // 如果当前已有活跃手势，先强制结束它.
        if (this.Active != null)
        {
            this.Active.ForcedFinish();
        }

        // 创建新令牌并更新活跃状态.
        GestureDeactivationToken gestureDeactivationToken = new GestureDeactivationToken(element, gestureName);
        this.Active = new GestureTuple(gestureName, forcedFinish, gestureDeactivationToken);

        return gestureDeactivationToken;
    }

    /// <summary>
    /// 强制停用指定的手势.
    /// </summary>
    /// <param name="gestureName">手势名称.</param>
    public void DeactivateGestureForcibly(string gestureName)
    {
        ArgumentNullException.ThrowIfNull(gestureName, nameof(gestureName));

        // 验证一致性：当前必须有活跃手势且名称匹配.
        if (this.Active == null || this.Active.GestureName != gestureName)
        {
            throw new InvalidOperationException("Gesture inconsistency. The current gesture is not active.");
        }

        // 触发强制结束回调并清空活跃状态.
        this.Active.ForcedFinish();
        this.Active = null;
    }

    /// <summary>
    /// 主动（自愿）停用手势.
    /// </summary>
    /// <param name="token">手势停用令牌.</param>
    public void DeactivateGestureWillingly(GestureDeactivationToken token)
    {
        ArgumentNullException.ThrowIfNull(token, nameof(token));

        // 验证手势名称是否匹配.
        if (this.Active == null || this.Active.GestureName != token.GestureName)
        {
            throw new InvalidOperationException("Gesture inconsistency. The gesture is not active.");
        }

        // 验证令牌对象是否一致，防止越权或错误的令牌操作.
        if (this.Active.Token != token)
        {
            throw new InvalidOperationException("Invalid GestureDeactivationToken used. Consider using the DeactivateGestureForcibly method.");
        }

        this.Active = null;
    }

    /// <summary>
    /// 触发活跃手势变更事件.
    /// </summary>
    private void OnActiveChanged()
    {
        this.ActiveGestureChanged?.Invoke(this, EventArgs.Empty);
    }
}