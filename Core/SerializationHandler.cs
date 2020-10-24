using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// <author>Michał Warzecha</author>
/// </summary>

namespace GameSerialization
{
    [Serializable]
    public class SerializerEnvelope : ISerializable
    {
        private Action<SerializationInfo> onSaveMethod;
        public SerializationInfo serializationInfo;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            onSaveMethod(info);
        }

        public SerializerEnvelope(Action<SerializationInfo> onSaveMethod)
        {
            this.onSaveMethod = onSaveMethod;
        }

        public SerializerEnvelope(SerializationInfo info, StreamingContext context)
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

        public static void SaveData(string saveName, IDataSerializer dataSerializer, bool append = false)
        {
            SaveData(saveName, dataSerializer.SaveDataToSerializer, append);
        }

        public static void SaveData(string saveName, Action<SerializationInfo> onSaveMethod, bool append = false)
        {
            string path = Application.persistentDataPath + saveFolder;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path += saveName + ".dat";
            using (FileStream fileStream = new FileStream(path, append ? FileMode.Append : FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, new SerializerEnvelope(onSaveMethod));
                fileStream.Close();
            }
        }

        public static void LoadData(string saveName, IDataSerializer dataSerializer)
        {
            LoadData(saveName, dataSerializer.LoadDataFromSerializer);
        }

        public static void LoadData(string saveName, Action<SerializationInfo> onLoadMethod)
        {
            string path = Application.persistentDataPath + saveFolder + saveName + ".dat";
            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    SerializerEnvelope serializerEnvelope = (SerializerEnvelope)formatter.Deserialize(fileStream);
                    onLoadMethod(serializerEnvelope.serializationInfo);
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
