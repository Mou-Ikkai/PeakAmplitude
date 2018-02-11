﻿// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections;

public class FretboardWrapMovement : MonoBehaviour {
    Renderer ren;
    float prevYPos;
    float prevHyperspeed;

    // Use this for initialization
    void Start () {
        ren = GetComponent<Renderer>();
        prevYPos = transform.position.y;
        prevHyperspeed = GameSettings.hyperspeed * GameSettings.gameSpeed;
        ren.sharedMaterial.mainTextureOffset = Vector2.zero;
    }

    void OnApplicationQuit()
    {
        // Reset purely for editor
        ren.sharedMaterial.mainTextureOffset = Vector2.zero;
    }

    void LateUpdate()
    {
        if (Globals.applicationMode == Globals.ApplicationMode.Playing)
        {
            Vector2 offset = ren.sharedMaterial.mainTextureOffset;
            offset.y += ((transform.position.y - prevYPos) / transform.localScale.y);
            ren.sharedMaterial.mainTextureOffset = offset;

            prevYPos = transform.position.y;
            prevHyperspeed = GameSettings.hyperspeed * GameSettings.gameSpeed;
        }
    }
    
	// Update is called once per frame
	void FixedUpdate () {
        if (Globals.applicationMode == Globals.ApplicationMode.Editor)  
        {
            if ((int)(prevHyperspeed * 100) == (int)((GameSettings.hyperspeed * GameSettings.gameSpeed) * 100))
            {
                Vector2 offset = ren.sharedMaterial.mainTextureOffset;
                offset.y += (transform.position.y - prevYPos) / transform.localScale.y;
                ren.sharedMaterial.mainTextureOffset = offset;
            }

            prevYPos = transform.position.y;
            prevHyperspeed = GameSettings.hyperspeed * GameSettings.gameSpeed;
        } 
    }
}
