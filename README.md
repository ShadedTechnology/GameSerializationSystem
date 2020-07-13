# Game Serialization System

Game data serialization system for Unity game engine.

## How to use:

First make sure to use **GameSerialization** namespace:
```C#
using GameSerialization;
```
#### You can save your data in three ways:

- using **SaveableProperty** attribute for your properties:
```C#
[SaveableProperty] public Vector3 vectorToSave {get; set;}
```

- using **SaveableField** attribute for your fields:
```C#
[SaveableField] public Vector3 vectorToSave;
```

- implementing **IOnSaveGameMethod** and **IOnLoadGameMethod** interfaces for your class:
```C#
public class ExampleSaveDataClass : MonoBehaviour, IOnSaveGameMethod, IOnLoadGameMethod
{
    private float dataToSave;

    public void OnSaveGameMethod(SerializationInfo info)
    {
        SerializationHelper.SaveObject(info, this, dataToSave, typeof(float));
    }

    public void OnLoadGameMethod(SerializationInfo info)
    {
        SerializationHelper.LoadObject(info, this);
    }
}
```

### To trigger saving of your scene call:
```C#
public SavingManager savingManager;

...

savingManager.SaveGame(saveName);
```


### To trigger loading of your scene call:
```C#
public SavingManager savingManager;

...

savingManager.LoadGame(saveName);
```

