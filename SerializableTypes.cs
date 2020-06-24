using System;
using UnityEngine;

// made by Michał Warzecha 28.08.2019

namespace GameSerialization
{
    [Serializable]
    public class Vector3Data
    {
        public float x, y, z;
        public Vector3Data(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public static implicit operator Vector3(Vector3Data v) => new Vector3(v.x, v.y, v.z);
        public static implicit operator Vector3Data(Vector3 v) => new Vector3Data(v);
    }

    public class Vector3Serializer : ISerializer
    {
        public object FromSerializable(object obj)
        {
            return (Vector3)(obj as Vector3Data);
        }

        public object ToSerializable(object obj)
        {
            return new Vector3Data((Vector3)obj);
        }

        public Type GetSerializationType()
        {
            return typeof(Vector3Data);
        }
    }

    [Serializable]
    public class QuaternionData
    {
        public float x, y, z, w;
        public QuaternionData(Quaternion q)
        {
            this.x = q.x;
            this.y = q.y;
            this.z = q.z;
            this.w = q.w;
        }

        public static implicit operator Quaternion(QuaternionData q) => new Quaternion(q.x, q.y, q.z, q.w);
        public static implicit operator QuaternionData(Quaternion q) => new QuaternionData(q);
    }

    public class QuaternionSerializer : ISerializer
    {
        public object FromSerializable(object obj)
        {
            return (Quaternion)(obj as QuaternionData);
        }

        public object ToSerializable(object obj)
        {
            return new QuaternionData((Quaternion)obj);
        }

        public Type GetSerializationType()
        {
            return typeof(QuaternionData);
        }
    }

    [Serializable]
    public class TransformData
    {
        public Vector3Data position;
        public QuaternionData rotation;
        public Vector3Data scale;

        public TransformData(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            scale = transform.localScale;
        }

        public void ApplyToTransform(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;
        }

        public static implicit operator TransformData(Transform q) => new TransformData(q);
    }

}