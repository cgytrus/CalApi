using System;
using System.Collections;

using HarmonyLib;

using ProphecySystem;

using UnityEngine;

namespace CalApi.API;

public abstract class BaseProphecy : MonoBehaviour {
    private static readonly Action<Prophet, int, int> moveProphecyInList =
        (Action<Prophet, int, int>)Delegate.CreateDelegate(typeof(Action<Prophet, int, int>),
            AccessTools.Method(typeof(Prophet), "MoveProphecyInList"));

    private static readonly Action<Prophet, int, bool> deleteProphecyFromList =
        (Action<Prophet, int, bool>)Delegate.CreateDelegate(typeof(Action<Prophet, int, bool>),
            AccessTools.Method(typeof(Prophet), "DeleteProphecyFromList"));

    public virtual bool skipNext => false;

    public abstract IEnumerator Performer(Prophet prophet, int index);

    public void MoveProphecyUp() {
        Prophet prophet = GetComponent<Prophet>();
        moveProphecyInList(prophet, prophet.prophecies.IndexOf(this), -1);
    }

    public void MoveProphecyDown() {
        Prophet prophet = GetComponent<Prophet>();
        moveProphecyInList(prophet, prophet.prophecies.IndexOf(this), 1);
    }

    public void DeleteProphecy() {
        Prophet prophet = GetComponent<Prophet>();
        deleteProphecyFromList(prophet, prophet.prophecies.IndexOf(this), true);
    }
}
