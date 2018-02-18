﻿// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

//#define TIMING_DEBUG

using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class Chart  {
    Song _song;
    List<ChartObject> _chartObjects;
    int _note_count;
    public string name = string.Empty;

    /// <summary>
    /// Read only list of notes.
    /// </summary>
    public Note[] notes { get; private set; }
    /// <summary>
    /// Read only list of starpower.
    /// </summary>
    public Starpower[] starPower { get; private set; }
    /// <summary>
    /// Read only list of local events.
    /// </summary>
    public ChartEvent[] events { get; private set; }
    /// <summary>
    /// The song this chart is connected to.
    /// </summary>
    public Song song { get { return _song; } }

    /// <summary>
    /// Read only list containing all chart notes, starpower and events.
    /// </summary>
    public ChartObject[] chartObjects { get { return _chartObjects.ToArray(); } }

    /// <summary>
    /// The total amount of notes in the chart, counting chord (notes sharing the same tick position) as a single note.
    /// </summary>
    public int note_count { get { return _note_count; } }

    /// <summary>
    /// Creates a new chart object.
    /// </summary>
    /// <param name="song">The song to associate this chart with.</param>
    /// <param name="name">The name of the chart (easy single, expert double guitar, etc.</param>
    public Chart (Song song, string name = "")
    {
        _song = song;
        _chartObjects = new List<ChartObject>();

        notes = new Note[0];
        starPower = new Starpower[0];
        events = new ChartEvent[0];

        _note_count = 0;

        this.name = name;
    }

    public Chart(Chart chart, Song song)
    {
        _song = song;
        name = chart.name;

        _chartObjects = new List<ChartObject>();
        _chartObjects.AddRange(chart._chartObjects);

        this.name = chart.name;
    }

    /// <summary>
    /// Updates all read-only values and the total note count.
    /// </summary>
    public void UpdateCache()
    {
        notes = _chartObjects.OfType<Note>().ToArray();
        starPower = _chartObjects.OfType<Starpower>().ToArray();
        events = _chartObjects.OfType<ChartEvent>().ToArray();

        _note_count = GetNoteCount();
    }

    int GetNoteCount()
    {
        if (notes.Length > 0)
        {
            int count = 1;

            uint previousPos = notes[0].position;
            for (int i = 1; i < notes.Length; ++i)
            {
                if (notes[i].position > previousPos)
                {
                    ++count;
                    previousPos = notes[i].position;
                }
            }

            return count;
        }
        else
            return 0;
    }

    public void SetCapacity(int size)
    {
        if (size > _chartObjects.Capacity)
            _chartObjects.Capacity = size;
    }

    public void Clear()
    {
        _chartObjects.Clear();
    }

    /// <summary>
    /// Adds a series of chart objects (note, starpower and/or chart events) into the chart.
    /// </summary>
    /// <param name="chartObjects">Items to add.</param>
    public void Add(ChartObject[] chartObjects)
    {
        foreach (ChartObject chartObject in chartObjects)
        {
            Add(chartObject, false);        
        }

        UpdateCache();
        ChartEditor.isDirty = true;
    }

    /// <summary>
    /// Adds a chart object (note, starpower and/or chart event) into the chart.
    /// </summary>
    /// <param name="chartObject">The item to add</param>
    /// <param name="update">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when adding multiple objects as it increases performance dramatically.</param>
    public int Add(ChartObject chartObject, bool update = true)
    {
        chartObject.chart = this;
        chartObject.song = this._song;

        int pos = SongObjectHelper.Insert(chartObject, _chartObjects);

        if (update)
            UpdateCache();

        ChartEditor.isDirty = true;

        return pos;
    }

    /// <summary>
    /// Removes a series of chart objects (note, starpower and/or chart events) from the chart.
    /// </summary>
    /// <param name="chartObjects">Items to add.</param>
    public void Remove(ChartObject[] chartObjects)
    {
        foreach (ChartObject chartObject in chartObjects)
        {
            Remove(chartObject, false);
        }

        UpdateCache();
        ChartEditor.isDirty = true;
    }

    /// <summary>
    /// Removes a chart object (note, starpower and/or chart event) from the chart.
    /// </summary>
    /// <param name="chartObject">Item to add.</param>
    /// <param name="update">Automatically update all read-only arrays? 
    /// If set to false, you must manually call the updateArrays() method, but is useful when removing multiple objects as it increases performance dramatically.</param>
    /// <returns>Returns whether the removal was successful or not (item may not have been found if false).</returns>
    public bool Remove(ChartObject chartObject, bool update = true)
    {
        bool success = SongObjectHelper.Remove(chartObject, _chartObjects);

        if (success)
        {
            chartObject.chart = null;
            chartObject.song = null;
            ChartEditor.isDirty = true;
        }

        if (update)
            UpdateCache();

        return success;
    }
}
