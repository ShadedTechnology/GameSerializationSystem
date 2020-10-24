using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <author>Michał Warzecha</author>
/// </summary>

namespace GameSerialization
{
    public class SerializableTypes
    {
        public static Dictionary<Type, ITypeSerializer> serializationDictionary = new Dictionary<Type, ITypeSerializer>()
        {
            { typeof(Vector2), new Vector2Serializer()},
            { typeof(Vector3), new Vector3Serializer()},
            { typeof(Vector4), new Vector4Serializer()},
            { typeof(Quaternion), new QuaternionSerializer()}
        };
    }

    [Serializable]
    public class Vector2Data
    {
        public float x, y;
        public Vector2Data(Vector2 v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        public static implicit operator Vector2(Vector2Data v) => new Vector2(v.x, v.y);
        public static implicit operator Vector2Data(Vector2 v) => new Vector2Data(v);
    }

    public class Vector2Serializer : ITypeSerializer
    {
        public object FromSerializable(object obj)
        {
            return (Vector2)(obj as Vector2Data);
        }

        public object ToSerializable(object obj)
        {
            return new Vector2Data((Vector2)obj);
        }

        public Type GetSerializationType()
        {
            return typeof(Vector2Data);
        }
    }

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

    public class Vector3Serializer : ITypeSerializer
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
    public class Vector4Data
    {
        public float x, y, z, w;
        public Vector4Data(Vector4 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.z = v.w;
        }

        public static implicit operator Vector4(Vector4Data v) => new Vector4(v.x, v.y, v.z);
        public static implicit operator Vector4Data(Vector3 v) => new Vector4Data(v);
    }

    public class Vector4Serializer : ITypeSerializer
    {
        public object FromSerializable(object obj)
        {
            return (Vector4)(obj as Vector4Data);
        }

        public object ToSerializable(object obj)
        {
            return new Vector4Data((Vector4)obj);
        }

        public Type GetSerializationType()
        {
            return typeof(Vector4Data);
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

    public class QuaternionSerializer : ITypeSerializer
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