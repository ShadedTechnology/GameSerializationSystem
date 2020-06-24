using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSerialization;

public class SavingManager : MonoBehaviour
{
    private SerializableObjectsContainer serializableObjectContainer;
    
    private void Start()
    {
        serializableObjectContainer = GameDataCollector.FindAllSerializableObjects();
    }

    public void SaveGame(string saveName)
    {
        SerializationHandler.SaveGame(saveName, new GameDataSerializer(serializableObjectContainer));
    }

    public void LoadGame(string saveName)
    {
        SerializationHandler.LoadGame(saveName, new GameDataSerializer(serializableObjectContainer));
    }
}
