using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// made by Michał Warzecha 28.08.2019

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public class SaveableField : Attribute {}

[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public class SaveableProperty : Attribute {}

public interface IOnSaveGameMethod
{
    void OnSaveGameMethod(SerializationInfo info);
}

public interface IOnLoadGameMethod
{
    void OnLoadGameMethod(SerializationInfo info);
}

static class TransformPathExtension
{
    public static string GetPath(this Transform current)
    {
        if (current.parent == null)
            return "/" + current.name;
        return current.parent.GetPath() + "/" + current.name;
    }
}

public interface ISerializer
{
    object ToSerializable(object obj);
    object FromSerializable(object obj);
    Type GetSerializationType();
}

static class SerializationHelper
{
    public static Dictionary<Type, ISerializer> serializationDictionary = new Dictionary<Type, ISerializer>()
    {
        { typeof(Vector3), new Vector3Serializer()},
        { typeof(Quaternion), new QuaternionSerializer()}
    };

    public static string GetComponentPath(MonoBehaviour component)
    {
        return component.transform.GetPath() + "/" + component.GetType().ToString();
    }

    public static void SaveObject(SerializationInfo info, string id, object obj, Type type)
    {
        info.AddValue(id + "_type", type);
        info.AddValue(id, obj);
    }

    public static void LoadObject(SerializationInfo info, string id, Action<object> callback)
    {
        Type type = info.GetValue(id + "_type", typeof(Type)) as Type;
        callback(info.GetValue(id, type));
    }
    
}

[Serializable]
class GameSerializer : ISerializable
{

    private void IterateTroughComponents(Action<MonoBehaviour, string> onComponent,
                                         Func<FieldInfo, string, MonoBehaviour, bool> onField,
                                         Func<PropertyInfo, string, MonoBehaviour, bool> onProperty)
    {
        foreach (MonoBehaviour component in GameObject.FindObjectsOfType<MonoBehaviour>())
        {
            var fields = component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(field => field.IsDefined(typeof(SaveableField), false)).ToArray();
            var props = component.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Where(prop => prop.IsDefined(typeof(SaveableProperty), false)).ToArray();
            string id = SerializationHelper.GetComponentPath(component);

            onComponent.Invoke(component, id);

            foreach (FieldInfo field in fields)
            {
                if(!onField.Invoke(field, id, component))
                {
                    Debug.LogError("Serialization error: " + id + " > Field \"" + field.Name + "\" cannot be serialized or deserialized");
                }
            }

            foreach (PropertyInfo prop in props)
            {
                if(!onProperty.Invoke(prop, id, component))
                {
                    Debug.LogError("Serialization error: " + id + " > Property \"" + prop.Name + "\" cannot be serialized or deserialized");
                }
            }
        }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("SceneManager_ActiveSceneId", SceneManager.GetActiveScene().buildIndex);
        Action<MonoBehaviour, string> onComponent = (MonoBehaviour component, string id) => {
            if(component is IOnSaveGameMethod)
            {
                (component as IOnSaveGameMethod).OnSaveGameMethod(info);
            }
        };
        Func<FieldInfo, string, MonoBehaviour, bool> onField = (field, id, component) => {
            if (field.FieldType.IsSerializable)
            {
                info.AddValue(id + field.Name, field.GetValue(component));
                return true;
            }
            if (SerializationHelper.serializationDictionary.TryGetValue(field.FieldType, out ISerializer serializer))
            {
                info.AddValue(id + field.Name, serializer.ToSerializable(field.GetValue(component)));
                return true;
            }
            return false;
        };
        Func<PropertyInfo, string, MonoBehaviour, bool> onProperty = (prop, id, component) => {
            if (prop.PropertyType.IsSerializable)
            {
                info.AddValue(id + prop.Name, prop.GetValue(component));
                return true;
            }
            if (SerializationHelper.serializationDictionary.TryGetValue(prop.PropertyType, out ISerializer serializer))
            {
                info.AddValue(id + prop.Name, serializer.ToSerializable(prop.GetValue(component)));
                return true;
            }
            return false;
        };
        IterateTroughComponents(onComponent, onField, onProperty);
    }

    public GameSerializer() { }

    public GameSerializer(SerializationInfo info, StreamingContext context)
    {
        Action<AsyncOperation> onSceneLoaded = (operation) =>
        {
            Action<MonoBehaviour, string> onComponent = (MonoBehaviour component, string id) =>
            {
                if (component is IOnLoadGameMethod)
                {
                    (component as IOnLoadGameMethod).OnLoadGameMethod(info);
                }
            };
            Func<FieldInfo, string, MonoBehaviour, bool> onField = (field, id, component) =>
            {
                if (field.FieldType.IsSerializable)
                {
                    field.SetValue(component, info.GetValue(id + field.Name, field.FieldType));
                    return true;
                }
                if (SerializationHelper.serializationDictionary.TryGetValue(field.FieldType, out ISerializer serializer))
                {
                    field.SetValue(component, serializer.FromSerializable(info.GetValue(id + field.Name, serializer.GetSerializationType())));
                    return true;
                }
                return false;
            };
            Func<PropertyInfo, string, MonoBehaviour, bool> onProperty = (prop, id, component) =>
            {
                if (prop.PropertyType.IsSerializable)
                {
                    prop.SetValue(component, info.GetValue(id + prop.Name, prop.PropertyType));
                    return true;
                }
                if (SerializationHelper.serializationDictionary.TryGetValue(prop.PropertyType, out ISerializer serializer))
                {
                    prop.SetValue(component, serializer.FromSerializable(info.GetValue(id + prop.Name, serializer.GetSerializationType())));
                    return true;
                }
                return false;
            };
            IterateTroughComponents(onComponent, onField, onProperty);
        };

        int sceneId = (int)info.GetValue("SceneManager_ActiveSceneId", typeof(int));
        if(sceneId != SceneManager.GetActiveScene().buildIndex)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);
            asyncLoad.completed += onSceneLoaded;
        } else
        {
            onSceneLoaded.Invoke(null);
        }
    }
}


public class SerializationHandler
{
    private const string saveFolder = "/GameSaves/";

    public static bool CheckIfSaveExists(string saveName)
    {
        string path = Application.persistentDataPath + saveFolder + saveName + ".dat";
        return (File.Exists(path));
    }
    
    public static void SaveGame(string saveName)
    {
        string path = Application.persistentDataPath + saveFolder;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        path += saveName + ".dat";
        using (FileStream fileStream = new FileStream(path, FileMode.Create))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fileStream, new GameSerializer());
            fileStream.Close();
        }

    }

    public static void LoadGame(string saveName)
    {
        string path = Application.persistentDataPath + saveFolder + saveName + ".dat";
        if (File.Exists(path))
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                GameSerializer dict = (GameSerializer)formatter.Deserialize(fileStream);
                fileStream.Close();
            }
        }
        else
        {
            Debug.LogError("Loading data failed! File doesn't exists at path: " + path);
        }
    }
}
