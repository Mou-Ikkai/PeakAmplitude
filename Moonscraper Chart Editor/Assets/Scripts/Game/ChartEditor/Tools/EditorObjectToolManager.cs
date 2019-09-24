﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EditorObjectToolManager : System.Object
{
    public enum ToolID
    {
        None,

        Cursor,
        Eraser,
        Note,
        Starpower,
        Event,
        BPM,
        TimeSignature,
        Section,
    }

    public enum ViewModeMaybe
    {
        None,
        Chart,
        Song,
    }

    bool ViewModeMaybeEqualsViewMode(ViewModeMaybe viewModeMaybe, Globals.ViewMode globalViewMode)
    {
        return
            (viewModeMaybe == ViewModeMaybe.Chart && globalViewMode == Globals.ViewMode.Chart) ||
            (viewModeMaybe == ViewModeMaybe.Song && globalViewMode == Globals.ViewMode.Song);
    }

    [System.Serializable]
    public class ToolConfig : System.Object
    {
        public ToolObject toolObject;
        public ViewModeMaybe viewToggle;
    }

    [SerializeField]
    ToolConfig[] tools;
    ToolConfig currentTool = null;
    public bool isToolActive { get; private set; }
    const ToolID DEFAULT_TOOL = ToolID.Cursor;

    public ToolID currentToolId
    {
        get
        {
            if (currentTool != null)
            {
                return currentTool.toolObject.GetTool();
            }

            return ToolID.None;
        }
    }

    ToolConfig GetToolConfigForId(ToolID id)
    {
        foreach (ToolConfig toolConfig in tools)
        {
            if (toolConfig.toolObject.GetTool() == id)
            {
                return toolConfig;
            }
        }

        return null;
    }

    public void Init()
    {
        isToolActive = false;

        ChartEditor.Instance.events.viewModeSwitchEvent.Register(OnViewModeSwitch);
        ChartEditor.Instance.RegisterPersistentSystem(ChartEditor.State.Editor, new ToolActiveListener(this));
        ChangeTool(DEFAULT_TOOL);
    }

    void OnViewModeSwitch(in Globals.ViewMode viewMode)
    {
        ToolConfig toolConfig = GetToolConfigForId(currentToolId);

        if (toolConfig != null)
        {
            ViewModeMaybe viewModeMaybe = toolConfig.viewToggle;
            if (viewModeMaybe != ViewModeMaybe.None && !ViewModeMaybeEqualsViewMode(viewModeMaybe, viewMode))
            {
                ChangeTool(DEFAULT_TOOL);
            }
        }
    }

    public void ChangeTool(ToolID toolId)
    {
        ToolConfig newTool = GetToolConfigForId(toolId);

        if (currentTool != null)
        {
            currentTool.toolObject.ToolDisable();
            currentTool.toolObject.gameObject.SetActive(false);
        }

        currentTool = newTool;

        if (currentTool != null)
        {
            ChartEditor editor = ChartEditor.Instance;
            Globals.ViewMode currentViewMode = Globals.viewMode;

            currentTool.toolObject.gameObject.SetActive(true);
            currentTool.toolObject.ToolEnable();
            currentTool.toolObject.gameObject.SetActive(isToolActive);

            ViewModeMaybe viewModeMaybe = currentTool.viewToggle;
            if (viewModeMaybe != ViewModeMaybe.None && !ViewModeMaybeEqualsViewMode(viewModeMaybe, currentViewMode))
            {
                editor.globals.ToggleSongViewMode(viewModeMaybe == ViewModeMaybe.Song);
            }
        }

        ChartEditor.Instance.events.toolChangedEvent.Fire();
    }

    void SetToolActive(bool isActive)
    {
        if (isActive == isToolActive)
            return;

        isToolActive = isActive;

        if (currentTool != null)
        {
            currentTool.toolObject.gameObject.SetActive(isToolActive);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////

    class ToolActiveListener : SystemManagerState.System
    {
        bool wasInDeleteMode = false;
        bool waitForMouseRelease = false;
        EditorObjectToolManager toolManager;

        public ToolActiveListener(EditorObjectToolManager toolManager)
        {
            this.toolManager = toolManager;
        }

        bool ShouldToolBeActive(Services services, ToolID currentToolId)
        {
            bool mouseInToolArea = services.InToolArea;
            bool blockedByUI = MouseMonitor.IsUIUnderPointer();
            bool keysModeActive = GameSettings.keysModeEnabled;
            bool ctrlDraggingBpm = currentToolId == ToolID.BPM && MSE.Input.KeyboardDevice.ctrlKeyBeingPressed && Input.GetMouseButton(0);
            bool currentlyInDeleteMode = Input.GetMouseButton(1);

            if (waitForMouseRelease)
                waitForMouseRelease = Input.GetMouseButton(0);

            bool deleteMode = currentlyInDeleteMode || (wasInDeleteMode && Input.GetMouseButton(0)); // Handle case where we've just done a delete and we're releasing right click first instead of left click
            wasInDeleteMode = deleteMode;

            if (keysModeActive)
                return true;

            if (!waitForMouseRelease && mouseInToolArea && !deleteMode && !blockedByUI && !ctrlDraggingBpm)
            {
                return true;
            }

            return false;
        }

        public override void SystemEnter()
        {
            wasInDeleteMode = false;
            waitForMouseRelease = Input.GetMouseButton(0);
            toolManager.SetToolActive(false);  // Cannot be active for the first frame, specifically when clicking to exit a menu
        }

        public override void SystemUpdate()
        {
            ChartEditor editor = ChartEditor.Instance;
            
            bool isActive = ShouldToolBeActive(editor.services, toolManager.currentToolId);
            toolManager.SetToolActive(isActive);
        }

        public override void SystemExit()
        {
            toolManager.SetToolActive(false);
        }
    }
}
