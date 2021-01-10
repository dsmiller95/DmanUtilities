# Scene save system 0.1.0

## Scene-wide save system tool

Asset setup pattern:

- In scene
  - WorldSaveManager component on a gameobject in the scene. Hook up buttons or events to call its Save() and Load() functions
  - Components in the scene which own data which should be saved and loaded implement the ISaveableData interface
  - GameObjects which will instantiate prefabs underneath them should have a SaveablePrefabParent component, with a unique parent name string
  - All prefabs which you want to be saved and re-instantiated on load should have a SaveablePrefab component on their top level gameobject, and only be
    placed as a direct child of a SaveablePrefabParent object. Each prefab asset should have its own unique SaveablePrefabType scriptable object asset linked with it
- In assets
  - One SaveablePrefabRegistry, with every SaveablePrefabType inside
  - Every prefab which can be saved has a 1-to-1 mapping to SaveablePrefabType

## Changing the save folder path

To save to a different save folder, change the SaveContext.saveName singleton variable before save and/or load

## Reacting to save events

The SaveSystemHooks singleton exposes 4 events which get triggered at different parts during the save lifecycle. These can be useful to trigger extra setup or teardown, if your components require more advanced control over their state when saved or loaded. These hooks are:

- PreSave
- PostSave
- PreLoad
- PostLoad
