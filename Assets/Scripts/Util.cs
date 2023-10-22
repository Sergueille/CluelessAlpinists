using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public enum ActionType
{
    jump, jetpack, grappling, bomb, balloon
}

public static class Util
{
    public static void ShuffleArray<T>(T[] array)
    {
        for (int i = 0; i < array.Length - 1; i++)
        {
            int id = UnityEngine.Random.Range(i + 1, array.Length);
            T tmp = array[id];
            array[id] = array[i];
            array[i] = tmp;
        } 
    }

    public static void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int id = UnityEngine.Random.Range(i + 1, list.Count);
            T tmp = list[id];
            list[id] = list[i];
            list[i] = tmp;
        } 
    }
        public static void SetLayerWithChildren(GameObject go, int layer)
    {
        go.layer = layer;

        foreach (Transform t in go.transform)
        {
            SetLayerWithChildren(t.gameObject, layer);
        }
    }

    public static T GetComponentInParents<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();

        if (comp != null) 
            return comp;

        if (go.transform.parent != null) 
            return GetComponentInParents<T>(go.transform.parent.gameObject);

        return null;
    }
}

[Serializable]
public struct MovementDescr
{
    public float amplitude;
    public float duration;
    public LeanTweenType easeType;
    [System.NonSerialized] public LTDescr descr;
    [System.NonSerialized] public int tweenID;

    public LTDescr Do(System.Action<float> callback)
    {
        descr = LeanTween.value(0, amplitude, duration).setOnUpdate(callback).setEase(easeType);
        tweenID = descr.id;
        return descr;
    } 

    public LTDescr DoReverse(System.Action<float> callback)
    {
        descr = LeanTween.value(amplitude, 0, duration).setOnUpdate(callback).setEase(easeType);
        tweenID = descr.id;
        return descr;
    } 

    public LTDescr DoNormalized(System.Action<float> callback)
    {
        descr = LeanTween.value(0, 1, duration).setOnUpdate(callback).setEase(easeType);
        tweenID = descr.id;
        return descr;
    } 

    public LTDescr DoMovement(System.Action<Vector3> callback, Vector3 start, Vector3 end)
    {
        descr = LeanTween.value(0, 1, duration).setOnUpdate(t => callback(start * (1 - t) + end * t)).setEase(easeType);
        tweenID = descr.id;
        return descr;
    } 

    public bool TryCancel()
    {
        if (descr == null)
        {
            return false;
        }

        LeanTween.cancel(tweenID);
        descr = null;
        tweenID = -1;

        return true;
    }
}

public struct Chrono
{
    public float startTime;
    public bool enabled;

    public Chrono(char _whyDoesCShardAbsolutelyWantsAParameter = 'a')
    {
        startTime = Time.time;
        enabled = false;
    }

    public float Restart()
    {
        startTime = Time.time;
        enabled = true;
        return startTime;
    }

    public void Disable()
    {
        enabled = false;
    } 

    public float Get()
    {
        if (!enabled) return 0;

        return Time.time - startTime;
    }
} 
