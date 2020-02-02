﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RSDKv5;
using ManiacEditor.Actions;
using SharpDX.Direct3D9;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Scene = RSDKv5.Scene;

namespace ManiacEditor.Classes.Core
{
    public static class Solution
    {
        public static Tileconfig TileConfig;
        public static Scene.EditorTiles CurrentTiles;
        public static Scene.EditorScene CurrentScene;
        public static Classes.Core.Scene.EditorEntities Entities;
        public static StageConfig StageConfig;
        public static Gameconfig GameConfig;

        #region Layers
        public static Classes.Core.Scene.Sets.EditorLayer FGHigher => CurrentScene?.HighDetails;
        public static Classes.Core.Scene.Sets.EditorLayer FGHigh => CurrentScene?.ForegroundHigh;
        public static Classes.Core.Scene.Sets.EditorLayer FGLow => CurrentScene?.ForegroundLow;
        public static Classes.Core.Scene.Sets.EditorLayer FGLower => CurrentScene?.LowDetails;
        public static Classes.Core.Scene.Sets.EditorLayer ScratchLayer => CurrentScene?.Scratch;
        public static Classes.Core.Scene.Sets.EditorLayer EditLayerA { get; set; }
        public static Classes.Core.Scene.Sets.EditorLayer EditLayerB { get; set; }
        #endregion

        #region Screen Size
        public static int SceneWidth => (CurrentScene != null ? CurrentScene.Layers.Max(sl => sl.Width) * 16 : 0);
        public static int SceneHeight => (CurrentScene != null ? CurrentScene.Layers.Max(sl => sl.Height) * 16 : 0);
        #endregion

        public static void UnloadScene()
        {
            Classes.Core.Solution.CurrentScene?.Dispose();
            Classes.Core.Solution.CurrentScene = null;
            Classes.Core.Solution.StageConfig = null;
            ManiacEditor.Controls.Base.MainEditor.Instance.EditorStatusBar._levelIDLabel.Content = "Level ID: NULL";
            Classes.Core.SolutionState.LevelID = -1;
            Classes.Core.SolutionState.EncorePaletteExists = false;
            Classes.Core.SolutionState.EncoreSetupType = 0;
            ManiacEditor.Controls.Base.MainEditor.Instance.ManiacINI.ClearSettings();
            ManiacEditor.Controls.Base.MainEditor.Instance.userDefinedEntityRenderSwaps = new Dictionary<string, string>();
            ManiacEditor.Controls.Base.MainEditor.Instance.userDefinedSpritePaths = new List<string>();
            ManiacEditor.Controls.Base.MainEditor.Instance.EditorToolbar.EncorePaletteButton.IsChecked = false;
            ManiacEditor.Controls.Base.MainEditor.Instance.Paths.UnloadScene();
            Classes.Core.SolutionState.QuitWithoutSavingWarningRequired = false;

            if (Classes.Core.Solution.CurrentTiles != null) Classes.Core.Solution.CurrentTiles.Dispose();
            Classes.Core.Solution.CurrentTiles = null;

            ManiacEditor.Controls.Base.MainEditor.Instance.TearDownExtraLayerButtons();

            ManiacEditor.Controls.Base.MainEditor.Instance.Background = null;

            ManiacEditor.Controls.Base.MainEditor.Instance.Chunks = null;

            EditorAnimations.AnimationTiming.Clear();


            /*if (entitiesClipboard != null)
            {
                foreach (Classes.Edit.Scene.Sets.EditorEntity entity in entitiesClipboard)
                    entity.PrepareForExternalCopy();
            }*/


            // Clear local clipboards
            //TilesClipboard = null;
            ManiacEditor.Controls.Base.MainEditor.Instance.entitiesClipboard = null;

            Classes.Core.Solution.Entities = null;

            Classes.Core.SolutionState.Zoom = 1;
            Classes.Core.SolutionState.ZoomLevel = 0;

            ManiacEditor.Controls.Base.MainEditor.Instance.UndoStack.Clear();
            ManiacEditor.Controls.Base.MainEditor.Instance.RedoStack.Clear();

            ManiacEditor.Controls.Base.MainEditor.Instance.EditorToolbar.EditFGLow.ClearCheckedItems();
            ManiacEditor.Controls.Base.MainEditor.Instance.EditorToolbar.EditFGHigh.ClearCheckedItems();
            ManiacEditor.Controls.Base.MainEditor.Instance.EditorToolbar.EditFGLower.ClearCheckedItems();
            ManiacEditor.Controls.Base.MainEditor.Instance.EditorToolbar.EditFGHigher.ClearCheckedItems();
            ManiacEditor.Controls.Base.MainEditor.Instance.EditorToolbar.EditEntities.ClearCheckedItems();

            ManiacEditor.Controls.Base.MainEditor.Instance.ZoomModel.SetViewSize();

            ManiacEditor.Controls.Base.MainEditor.Instance.UI.UpdateControls();

            // clear memory a little more aggressively 
            ManiacEditor.Controls.Base.MainEditor.Instance.EntityDrawing.ReleaseResources();
            GC.Collect();
            Classes.Core.Solution.TileConfig = null;

            ManiacEditor.Controls.Base.MainEditor.Instance.UpdateStartScreen(true);
        }



    }
}