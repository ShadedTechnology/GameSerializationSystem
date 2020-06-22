using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

// made by Michał Warzecha 28.08.2019

public class TransformSaver : MonoBehaviour
{
    [SaveableProperty] TransformData transformData
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
