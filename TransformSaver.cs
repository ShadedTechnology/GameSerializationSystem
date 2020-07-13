using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using GameSerialization;

/// <summary>
/// <author>Michał Warzecha</author>
/// </summary>

public class TransformSaver : MonoBehaviour
{
    [SaveableProperty] private TransformData transformData
    {
        get
        {
            return transform;
        }
        set
        {
            value.ApplyToTransform(transform);
        }
    }
}
