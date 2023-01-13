# Stateful Manager

The goals of this extension are:

-   To simplify the encapsulation of state behaviour
-   Allow for the definition of reactions to state changes in a declarative manner
-   Allow for a reaction to be continuously called in the Update() method until the state changes and the reaction becomes obsolete

## Caveats

-   There might be a performance hit if you change your state quickly a lot, due to the internal usage of reflections
-   The reaction methods must be public to be invokable by StatefulManager
-   Only the Update() method is supported right now. But it should be pretty straightforward to add more built-in monobehaviour methods like LateUpdate(), FixedUpdate() etc.

## Usage

1. Create a states enum
2. Derive from StatefulManager
3. Create methods for handling state changes, using the following naming convention: "OnXXXUpdate" where XXX is the name of the state from the states enum.

## Example

Create a states enum and derive your class from StatefulManager like this:

```csharp
public enum BuildStates {
    Idle,
    Start,
}

public class BuildingManager : StatefulManager<BuildStates> {
    public void Build() {
        BuilderStateMachine.ChangeState(BuildStates.Start);
    }

    // Triggered on every Update()
    public void OnStartUpdate() {
        Debug.Log($"{name}: Start");
        // Do something continuously like tracking a cursor to place an object. When it's done, change state.
        BuilderStateMachine.ChangeState(StructureBuildStates.Idle);
    }

    // Triggered once
    public void OnIdle() {
        Debug.Log($"{name}: Idle.");
    }
}
```

Use it like this:

```csharp
BuildingManager buildingManager = new BuildingManager();
buildingManager.Build();

// -> Manager: Start
// -> Manager: Start
// -> Manager: Start
// -> Manager: Idle
```
