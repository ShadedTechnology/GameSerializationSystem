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

namespace GameSerialization
{
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

    public class FieldSerializationInfo
    {
        public string id;
        public FieldInfo fieldInfo;
        public MonoBehaviour component;
        public Func<object> getSerializableObject;
        public Func<SerializationInfo, object> getOriginalObject;
    }

    public class PropertySerializationInfo
    {
        public string id;
        public PropertyInfo propInfo;
        public MonoBehaviour component;
        public Func<object> getSerializableObject;
        public Func<SerializationInfo, object> getOriginalObject;
    }

    public class GameSerializableObjectContainer
    {
        public List<IOnSaveGameMethod> OnSaveGameMethods { get; set; } = new List<IOnSaveGameMethod>();
        public List<IOnLoadGameMethod> OnLoadGameMethods { get; set; } = new List<IOnLoadGameMethod>();
        public List<FieldSerializationInfo> SerializableFields { get; set; } = new List<FieldSerializationInfo>();
        public List<PropertySerializationInfo> SerializableProps { get; set; } = new List<PropertySerializationInfo>();

        private FieldSerializationInfo FieldToSerializationInfo(FieldInfo field, string id, MonoBehaviour component)
        {
            string fieldId = id + field.Name;
            Func<object> getSerializableObject;
            Func<SerializationInfo, object> getOriginalObject;
            if (field.FieldType.IsSerializable)
            {
                getSerializableObject = () => { return field.GetValue(component); };
                getOriginalObject = (info) => { return info.GetValue(fieldId, field.FieldType); };
            }
            else if (SerializationHelper.serializationDictionary.TryGetValue(field.FieldType, out ISerializer serializer))
            {
                getSerializableObject = () => { return serializer.ToSerializable(field.GetValue(component)); };
                getOriginalObject = (info) => { return serializer.FromSerializable(info.GetValue(fieldId, serializer.GetSerializationType())); };
            }
            else
            {
                Debug.LogError("Serialization error: " + id + " > Field \"" + field.Name + "\" cannot be serialized or deserialized");
                return null;
            }

            return new FieldSerializationInfo
            {
                id = fieldId,
                fieldInfo = field,
                component = component,
                getSerializableObject = getSerializableObject,
                getOriginalObject = getOriginalObject
            };
        }

        private PropertySerializationInfo PropToSerializationInfo(PropertyInfo prop, string id, MonoBehaviour component)
        {
            string propId = id + prop.Name;
            Func<object> getSerializableObject;
            Func<SerializationInfo, object> getOriginalObject;
            if (prop.PropertyType.IsSerializable)
            {
                getSerializableObject = () => { return prop.GetValue(component); };
                getOriginalObject = (info) => { return info.GetValue(propId, prop.PropertyType); };
            }
            else if (SerializationHelper.serializationDictionary.TryGetValue(prop.PropertyType, out ISerializer serializer))
            {
                getSerializableObject = () => { return serializer.ToSerializable(prop.GetValue(component)); };
                getOriginalObject = (info) => { return serializer.FromSerializable(info.GetValue(propId, serializer.GetSerializationType())); };
            }
            else
            {
                Debug.LogError("Serialization error: " + id + " > Property \"" + prop.Name + "\" cannot be serialized or deserialized");
                return null;
            }

            return new PropertySerializationInfo
            {
                id = propId,
                propInfo = prop,
                component = component,
                getSerializableObject = getSerializableObject,
                getOriginalObject = getOriginalObject
            };
        }

        public GameSerializableObjectContainer FindAllSerializableObjects()
        {
            foreach (MonoBehaviour component in GameObject.FindObjectsOfType<MonoBehaviour>())
            {
                var fields = component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Where(field => field.IsDefined(typeof(SaveableField), false)).ToArray();
                var props = component.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Where(prop => prop.IsDefined(typeof(SaveableProperty), false)).ToArray();
                string id = SerializationHelper.GetComponentPath(component);

                if (component is IOnSaveGameMethod)
                {
                    OnSaveGameMethods.Add((IOnSaveGameMethod)component);
                }
                if (component is IOnLoadGameMethod)
                {
                    OnLoadGameMethods.Add((IOnLoadGameMethod)component);
                }

                foreach (FieldInfo field in fields)
                {
                    FieldSerializationInfo fieldSerializationInfo = FieldToSerializationInfo(field, id, component);
                    if(null != fieldSerializationInfo)
                    {
                        SerializableFields.Add(fieldSerializationInfo);
                    }
                }

                foreach (PropertyInfo prop in props)
                {
                    PropertySerializationInfo propertySerializationInfo = PropToSerializationInfo(prop, id, component);
                    if(null != propertySerializationInfo)
                    {
                        SerializableProps.Add(propertySerializationInfo);
                    }
                }
            }
            return this;
        }

        private void LoadDataToGame(SerializationInfo info)
        {
            foreach (IOnLoadGameMethod onLoadGameMethod in OnLoadGameMethods)
            {
                onLoadGameMethod.OnLoadGameMethod(info);
            }
            foreach (FieldSerializationInfo fieldSerializationInfo in SerializableFields)
            {
                fieldSerializationInfo.fieldInfo.SetValue(fieldSerializationInfo.component, fieldSerializationInfo.getOriginalObject(info));
            }
            foreach (PropertySerializationInfo propSerializationInfo in SerializableProps)
            {
                propSerializationInfo.propInfo.SetValue(propSerializationInfo.component, propSerializationInfo.getOriginalObject(info));
            }
        }

        public void SaveDataFromGame(SerializationInfo info)
        {
            info.AddValue("SceneManager_ActiveSceneId", SceneManager.GetActiveScene().buildIndex);
            foreach (IOnSaveGameMethod onSaveGameMethod in OnSaveGameMethods)
            {
                onSaveGameMethod.OnSaveGameMethod(info);
            }
            foreach (FieldSerializationInfo fieldSerializationInfo in SerializableFields)
            {
                info.AddValue(fieldSerializationInfo.id, fieldSerializationInfo.getSerializableObject());
            }
            foreach (PropertySerializationInfo propSerializationInfo in SerializableProps)
            {
                info.AddValue(propSerializationInfo.id, propSerializationInfo.getSerializableObject());
            }
        }

        public void LoadSceneFromGameSerializer(GameSerializer serializer)
        {
            SerializationInfo info = serializer.serializationInfo;
            Action<AsyncOperation> onSceneLoaded = (operation) => LoadDataToGame(info);

            int sceneId = (int)info.GetValue("SceneManager_ActiveSceneId", typeof(int));
            if (sceneId != SceneManager.GetActiveScene().buildIndex)
            {
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneId);
                asyncLoad.completed += onSceneLoaded;
            }
            else
            {
                onSceneLoaded.Invoke(null);
            }
        }
    }

    [Serializable]
    public class GameSerializer : ISerializable
    {
        private GameSerializableObjectContainer serializationContainer;
        public SerializationInfo serializationInfo;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            serializationContainer.SaveDataFromGame(info);
        }

        public GameSerializer(GameSerializableObjectContainer serializationContainer)
        {
            this.serializationContainer = serializationContainer;
        }

        public GameSerializer(SerializationInfo info, StreamingContext context)
        {
            this.serializationInfo = info;
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
    
        public static void SaveGame(string saveName, GameSerializableObjectContainer serializableObjects)
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
                formatter.Serialize(fileStream, new GameSerializer(serializableObjects));
                fileStream.Close();
            }

        }

        public static void LoadGame(string saveName, GameSerializableObjectContainer serializableObjects)
        {
            string path = Application.persistentDataPath + saveFolder + saveName + ".dat";
            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    GameSerializer gameSerializer = (GameSerializer)formatter.Deserialize(fileStream);
                    serializableObjects.LoadSceneFromGameSerializer(gameSerializer);
                    fileStream.Close();
                }
            }
            else
            {
                Debug.LogError("Loading data failed! File doesn't exists at path: " + path);
            }
        }
    }

}
