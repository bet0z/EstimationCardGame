using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Tween the widget's depth.
/// </summary>

[RequireComponent(typeof(UIWidget))]
[AddComponentMenu("NGUI/Tween/Tween Text")]
public class TweenText : UITweener
{
    public int from = 100;
    public int to = 100;

    UILabel mWidget;

    public UILabel cachedWidget { get { if (mWidget == null) mWidget = GetComponent<UILabel>(); return mWidget; } }

    [System.Obsolete("Use 'value' instead")]
    public int num { get { return this.value; } set { this.value = value; } }

    /// <summary>
    /// Tween's current value.
    /// </summary>

    public int value { get { return Convert.ToInt32(cachedWidget.text); } set { cachedWidget.text = value.ToString(); } }

    /// <summary>
    /// Tween the value.
    /// </summary>

    protected override void OnUpdate(float factor, bool isFinished)
    {
        value = Mathf.RoundToInt(from * (1f - factor) + to * factor);
    }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenText Begin(UILabel widget, float duration, int num)
    {
        TweenText comp = UITweener.Begin<TweenText>(widget.gameObject, duration);
        //TweenDepth comp = UITweener.Begin<TweenDepth>(go, duration);
        comp.from = Convert.ToInt32(widget.text);
        comp.to = num;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }

    [ContextMenu("Set 'From' to current value")]
    public override void SetStartToCurrentValue() { from = value; }

    [ContextMenu("Set 'To' to current value")]
    public override void SetEndToCurrentValue() { to = value; }

    [ContextMenu("Assume value of 'From'")]
    void SetCurrentValueToStart() { value = from; }

    [ContextMenu("Assume value of 'To'")]
    void SetCurrentValueToEnd() { value = to; }
}