﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Whammy : MonoBehaviour {
    public float keyShiftSpeed = 5;
    public float widthMultiplier = 1;
    public float whammyLerpSpeed = 20;

    LineRenderer lineRenderer;
   // AnimationCurve lineCurve;

    float prevHeight = 0;

    public bool canWhammy = false;

    // Use this for initialization
    void Start () {
        lineRenderer = GetComponent<LineRenderer>();

        prevHeight = transform.localScale.y; 
	}

    
	// Update is called once per frame
	void Update () {
        AnimationCurve lineCurve = lineRenderer.widthCurve;

        if (Globals.applicationMode == Globals.ApplicationMode.Playing && transform.localScale.y > 0)
        {
            ShiftAnimationKeys(lineCurve, keyShiftSpeed * Time.deltaTime * (Globals.hyperspeed / Globals.gameSpeed) / transform.localScale.y);

            float whammyVal = (lerpedWhammyVal() + 1) * widthMultiplier;

            lineCurve.AddKey(new Keyframe(0, whammyVal + 1));
        }
        else
        {
            lineCurve.keys = new Keyframe[] { new Keyframe(0, 1) };           
        }

        lineRenderer.widthCurve = lineCurve;
        prevHeight = transform.localScale.y;
    }

    static void ShiftAnimationKeys(AnimationCurve lineCurve, float shiftDistance)
    {
        for (int i = lineCurve.keys.Length - 1; i >= 0; --i)
        {
            float keyTime = lineCurve.keys[i].time + shiftDistance;
            float keyValue = lineCurve.keys[i].value;

            if (keyTime <= 1)
                lineCurve.MoveKey(i, new Keyframe(keyTime, keyValue));
            else
                lineCurve.RemoveKey(i);
        }
    }

    public void ReduceSustainSizeKeysAdjust(float previousHeight, float newHeight)
    {
        if (newHeight >= previousHeight)
            return;

        AnimationCurve lineCurve = lineRenderer.widthCurve;

        if (prevHeight > 0)
            lineCurve.keys = KeyframeSizeReduction(lineCurve.keys, 1 - (newHeight / prevHeight));

        lineRenderer.widthCurve = lineCurve;
    }

    static Keyframe[] KeyframeSizeReduction(Keyframe[] keys, float timeCutoff)
    {       
        List<Keyframe> newKeys = new List<Keyframe>();

        if (timeCutoff <= 0)
            return keys;
        else if (timeCutoff > 1)
            return new Keyframe[] { new Keyframe(0, 1) };

        foreach(Keyframe key in keys)
        {
            if (key.time < timeCutoff)
                continue;
            else
            {
                float newTime = (key.time - timeCutoff) / (1 - timeCutoff);
                newKeys.Add(new Keyframe(newTime, key.value));
            }
        }

        if (newKeys.Count > 0)
            return newKeys.ToArray();
        else
        {
            
            return keys;
        }
    }

    float currentWhammyVal = -1;
    float lerpedWhammyVal()
    {
        float rawVal = Input.GetAxisRaw("Whammy");

        if (!canWhammy)
            currentWhammyVal = -1;
        else
        {
            if (rawVal > currentWhammyVal)
            {
                currentWhammyVal += whammyLerpSpeed * Time.deltaTime;
                if (currentWhammyVal > rawVal)
                    currentWhammyVal = rawVal;
            }
            else if (rawVal.Round(2) < currentWhammyVal)
            {
                currentWhammyVal -= whammyLerpSpeed * Time.deltaTime;
                if (currentWhammyVal < rawVal)
                    currentWhammyVal = rawVal;
            }
        }

        return currentWhammyVal;
    }
}
