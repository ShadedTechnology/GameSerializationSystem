using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSerialization;

public class SavingManager : MonoBehaviour
{
    private SerializableObjectsContainer serializableObjectContainer;
    
    private void Start()
    {
        serializableObjectContainer = new SerializableObjectsContainer().FindAllSerializableObjects();
    }

    public void SaveGame(string saveName)
    {
        SerializationHandler.SaveGame(saveName, new GameDataCollector(serializableObjectContainer));
    }

    public void LoadGame(string saveName)
    {
        SerializationHandler.LoadGame(saveName, new GameDataCollector(serializableObjectContainer));
    }
}
