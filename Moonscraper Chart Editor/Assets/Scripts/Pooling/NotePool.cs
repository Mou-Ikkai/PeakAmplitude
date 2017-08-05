﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePool : SongObjectPool
{
    public NotePool(GameObject parent, GameObject prefab, int initialSize) : base(parent, prefab, initialSize)
    {
        if (!prefab.GetComponentInChildren<NoteController>())
            throw new System.Exception("No NoteController attached to prefab");   
    }

    protected override void Assign(SongObjectController sCon, SongObject songObject)
    {
        NoteController controller = sCon as NoteController;

        // Assign pooled objects
        controller.note = (Note)songObject;
        controller.Activate();
        controller.gameObject.SetActive(true);
    }

    public void Activate(Note[] range, int index, int length)
    {
        base.Activate(range, index, length);
    }
}