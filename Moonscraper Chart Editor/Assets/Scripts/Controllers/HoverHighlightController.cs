﻿using UnityEngine;
using System.Collections.Generic;

public class HoverHighlightController : MonoBehaviour {
    public GameObject hoverHighlight;

    GameObject[] highlights = new GameObject[5];

	// Use this for initialization
	void Start () {
        for (int i = 0; i < highlights.Length; ++i)
        {
            highlights[i] = Instantiate(hoverHighlight);
            highlights[i].SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
        // Show a preview if the user will click on an object
        GameObject songObject = Mouse.GetSelectableObjectUnderMouse();
        foreach (GameObject highlight in highlights)
            highlight.SetActive(false);

        if (Globals.applicationMode == Globals.ApplicationMode.Editor && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && songObject != null)
        {
            List<GameObject> songObjects = new List<GameObject>();

            if (Input.GetButton("ChordSelect"))
            {
                // Check if we're over a note
                NoteController nCon = songObject.GetComponent<NoteController>();
                if (nCon)
                {
                    Note[] notes = nCon.note.GetChord();
                    foreach (Note note in notes)
                        songObjects.Add(note.controller.gameObject);
                }
                else
                {
                    SustainController sCon = songObject.GetComponent<SustainController>();
                    if (sCon)
                    {
                        Note[] notes = sCon.nCon.note.GetChord();
                        foreach (Note note in notes)
                            songObjects.Add(note.controller.sustain.gameObject);
                    }
                }
            }
            else
            {
                songObjects.Add(songObject);
            }

            for (int i = 0; i < songObjects.Count; ++i)
            {
                if (i < highlights.Length)
                {
                    highlights[i].SetActive(true);
                    highlights[i].transform.position = songObjects[i].transform.position;

                    Vector3 scale = songObjects[i].transform.localScale;
                    Collider col3d = songObjects[i].GetComponent<Collider>();
                    Collider2D col = songObjects[i].GetComponent<Collider2D>();

                    if (col3d)
                        scale = col3d.bounds.size;
                    else
                        scale = col.bounds.size;

                    if (scale.z == 0)
                        scale.z = 0.1f;
                    highlights[i].transform.localScale = scale;
                }
            }
        }
    }
}