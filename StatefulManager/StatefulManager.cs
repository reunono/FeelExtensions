using System;
using System.Reflection;
using UnityEngine;
using MoreMountains.Tools;

/// <summary>
/// An abstract stateful manager that allows derived classes to easily manage and define reactions to state changes in a declarative manner.
/// </summary>
public abstract class StatefulManager<T> : MonoBehaviour, MMEventListener<MMStateChangeEvent<T>>
    where T : struct, IComparable, IConvertible, IFormattable
{
    protected MMStateMachine<T> StateMachine { get; private set; }

    private Action methodOnUpdate;

    protected void Awake()
    {
        StateMachine = new(gameObject, true);
    }

    private void Update()
    {
        methodOnUpdate?.Invoke();
    }

    private void OnEnable()
    {
        MMEventManager.AddListener(this);
    }

    private void OnDisable()
    {
        MMEventManager.RemoveListener(this);
    }

    public void OnMMEvent(MMStateChangeEvent<T> stateChangeEvent)
    {
        string newState = stateChangeEvent.NewState.ToString();
        string methodNameImmediate = $"On{newState}";
        string methodNameUpdate = $"On{newState}Update";

        // If exists, invoke this method immediately. This is a regular on-off state change event.
        GetType().GetMethod(methodNameImmediate)?.Invoke(this, null);

        // If exists, set this method to be triggered on every Update().
        MethodInfo methodInfo = GetType().GetMethod(methodNameUpdate);
        methodOnUpdate = (Action)methodInfo?.CreateDelegate(typeof(Action), this);

        // You can add more built-in monobehaviour methods if you want.
        //
        // Example:
        //
        // string methodNameFixedUpdate = $"On{newState}FixedUpdate";
        // MethodInfo methodInfoFixedUpdate = GetType().GetMethod(methodNameFixedUpdate);
        // methodOnFixedUpdate = (Action)methodInfoFixedUpdate?.CreateDelegate(typeof(Action), this);
        //
        // In this case, you would have to add the property `Action methodOnFixedUpdate;`
        // and the method `private void FixedUpdate() { methodOnFixedUpdate?.Invoke(); }`
    }
}
