using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

// made by Michał Warzecha 28.08.2019

namespace GameSerialization
{
    [Serializable]
    public class GameSerializer : ISerializable
    {
        private IDataCollector dataCollector;
        public SerializationInfo serializationInfo;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            dataCollector.SaveDataToSerializer(info);
        }

        public GameSerializer(IDataCollector dataCollector)
        {
            this.dataCollector = dataCollector;
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
    
        public static void SaveGame(string saveName, IDataCollector dataCollector)
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
                formatter.Serialize(fileStream, new GameSerializer(dataCollector));
                fileStream.Close();
            }
        }

        public static void SaveGameAppend(string saveName, IDataCollector dataCollector)
        {
            string path = Application.persistentDataPath + saveFolder;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path += saveName + ".dat";
            using (FileStream fileStream = new FileStream(path, FileMode.Append))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, new GameSerializer(dataCollector));
                fileStream.Close();
            }
        }

        public static void LoadGame(string saveName, IDataCollector dataCollector)
        {
            string path = Application.persistentDataPath + saveFolder + saveName + ".dat";
            if (File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    GameSerializer gameSerializer = (GameSerializer)formatter.Deserialize(fileStream);
                    dataCollector.LoadDataFromSerializer(gameSerializer.serializationInfo);
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
