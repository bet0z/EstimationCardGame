using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DragRotatorAxisInfo
{
    public DragRotatorAxisInfo(float multiplier, float minDegrees, float maxDegrees, float restSecond)
    {
        m_ForceMultiplier = multiplier;
        m_MinDegrees = minDegrees;
        m_MaxDegrees = maxDegrees;
        m_RestSeconds = restSecond;
    }
    public float m_ForceMultiplier;
    public float m_MinDegrees;
    public float m_MaxDegrees;
    public float m_RestSeconds;
}