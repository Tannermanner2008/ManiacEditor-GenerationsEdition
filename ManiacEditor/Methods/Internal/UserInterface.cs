﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RSDKv5;
using ManiacEditor.Actions;
using System.Windows.Controls;
using ManiacEditor.Controls.Base;
using ManiacEditor.Controls.Base.Toolbars;

namespace ManiacEditor.Methods.Internal
{
    public static class UserInterface
    {
        private static MainEditor Instance;
        public static void UpdateInstance(MainEditor _instance)
        {
            Instance = _instance;
        }

        public static void SetSceneOnlyButtonsState(bool enabled, bool stageLoad = false)
        {
            Instance.EditorToolbar.SetSceneOnlyButtonsState(enabled, stageLoad);
            Instance.EditorMenuBar.SetSceneOnlyButtonsState(enabled, stageLoad);
            Instance.EditorToolbar.UpdateGameRunningButton(enabled);

            SetEditButtonsState(enabled);
            UpdateTooltips();

            if (stageLoad)
            {
                Instance.ZoomModel.SetViewSize((int)(Classes.Editor.Solution.SceneWidth * Classes.Editor.SolutionState.Zoom), (int)(Classes.Editor.Solution.SceneHeight * Classes.Editor.SolutionState.Zoom));
            }

            Methods.Internal.Theming.UpdateButtonColors();

        }
        public static void SetParallaxAnimationOnlyButtonsState(bool enabled = true)
        {
            Instance.EditorToolbar.Open.IsEnabled = !enabled;
            Instance.EditorToolbar.ShowAnimations.IsEnabled = enabled || Classes.Editor.Solution.CurrentScene != null;
            Instance.EditorToolbar.animationsSplitButton_Dropdown.IsEnabled = enabled || Classes.Editor.Solution.CurrentScene != null;
            Instance.EditorMenuBar.MenuBar.IsEnabled = !enabled;
            Instance.EditorStatusBar.StatusBar1.IsEnabled = !enabled;
            Instance.EditorTabControl.IsEnabled = !enabled;
            Instance.EditorToolbar.New.IsEnabled = !enabled;
            Instance.EditorToolbar.Open.IsEnabled = !enabled;

            if (enabled)
            {
                Instance.EditorToolbar.ShowFGHigh.IsEnabled = Classes.Editor.Solution.FGHigh != null;
                Instance.EditorToolbar.ShowFGLow.IsEnabled = Classes.Editor.Solution.FGLow != null;
                Instance.EditorToolbar.ShowFGHigher.IsEnabled = Classes.Editor.Solution.FGHigher != null;
                Instance.EditorToolbar.ShowFGLower.IsEnabled = Classes.Editor.Solution.FGLower != null;
                Instance.EditorToolbar.ShowEntities.IsEnabled = true;
                Instance.LeftToolbarToolbox.SelectedIndex = -1;
                UpdateToolbars(false, false, false);
                SetEditButtonsState(false);
            }
            foreach (var elb in Instance.ExtraLayerEditViewButtons)
            {
                elb.Value.IsEnabled = !enabled;
            }
            Instance.LeftToolbarToolbox.IsEnabled = !enabled;
        }
        public static void SetSelectOnlyButtonsState(bool enabled = true)
        {
            Instance.EditorMenuBar.SetPasteButtonsState(true);
            Instance.EditorMenuBar.SetSelectOnlyButtonsState(enabled);
            enabled &= ManiacEditor.Classes.Editor.SolutionState.IsSelected();

            if (ManiacEditor.Classes.Editor.SolutionState.IsEntitiesEdit() && Instance.EntitiesToolbar != null)
            {
                Instance.EntitiesToolbar.SelectedEntities = Classes.Editor.Solution.Entities.SelectedEntities.Select(x => x.Entity).ToList();
            }
        }
        private static void SetEditButtonsState(bool enabled)
        {
            Instance.EditorToolbar.SetEditButtonsState(enabled);
            if (ManiacEditor.Classes.Editor.SolutionState.IsTilesEdit())
            {
                if (Instance.TilesToolbar == null)
                {
                    if (Classes.Editor.SolutionState.UseEncoreColors)
                        Instance.TilesToolbar = new ManiacEditor.Controls.Base.Toolbars.TilesToolbar.TilesToolbar(Classes.Editor.Solution.CurrentTiles, Instance.Paths.StageTiles_Source, Instance.EncorePalette[0], MainEditor.Instance);
                    else
                        Instance.TilesToolbar = new ManiacEditor.Controls.Base.Toolbars.TilesToolbar.TilesToolbar(Classes.Editor.Solution.CurrentTiles, Instance.Paths.StageTiles_Source, null, MainEditor.Instance);


                    Instance.TilesToolbar.TileDoubleClick = new Action<int>(x =>
                    {
                        Classes.Editor.EditorActions.EditorPlaceTile(new System.Drawing.Point((int)(Classes.Editor.SolutionState.ViewPositionX / Classes.Editor.SolutionState.Zoom) + Classes.Editor.Constants.TILE_SIZE - 1, (int)(Classes.Editor.SolutionState.ViewPositionY / Classes.Editor.SolutionState.Zoom) + Classes.Editor.Constants.TILE_SIZE - 1), x, Classes.Editor.Solution.EditLayerA);
                    });
                    Instance.TilesToolbar.TileOptionChanged = new Action<int, bool>((option, state) =>
                    {
                        Classes.Editor.Solution.EditLayerA?.SetPropertySelected(option + 12, state);
                        Classes.Editor.Solution.EditLayerB?.SetPropertySelected(option + 12, state);

                    });
                    Instance.ToolBarPanelRight.Children.Clear();
                    Instance.ToolBarPanelRight.Children.Add(Instance.TilesToolbar);
                    UpdateToolbars(true, true);
                    Instance.Editor_Resize(null, null);
                    Instance.Focus();
                }
                if (ManiacEditor.Classes.Editor.SolutionState.IsChunksEdit()) Instance.TilesToolbar.TabControl.TabIndex = 1;
                else Instance.TilesToolbar.TabControl.TabIndex = 0;
                UpdateTilesOptions();
                Instance.TilesToolbar.ShowShortcuts = Instance.EditorToolbar.DrawToolButton.IsChecked.Value;
            }
            else
            {
                if (Instance.TilesToolbar != null)
                {
                    Instance.TilesToolbar.Dispose();
                    Instance.TilesToolbar = null;
                    Instance.Focus();
                }
            }
            if (ManiacEditor.Classes.Editor.SolutionState.IsEntitiesEdit())
            {
                if (Instance.EntitiesToolbar == null)
                {
                    Instance.EntitiesToolbar = new ManiacEditor.Controls.Base.Toolbars.EntitiesToolbar.EntitiesToolbar(Classes.Editor.Solution.CurrentScene.Objects, MainEditor.Instance)
                    {
                        SelectedEntity = new Action<int>(x =>
                        {
                            Classes.Editor.Solution.Entities.SelectSlot(x);
                            SetSelectOnlyButtonsState();
                        }),
                        AddAction = new Action<ManiacEditor.Actions.IAction>(x =>
                        {
                            Instance.UndoStack.Push(x);
                            Instance.RedoStack.Clear();
                            UpdateControls();
                        }),
                        Spawn = new Action<SceneObject>(x =>
                        {
                            Classes.Editor.Solution.Entities.Add(x, GetEntitySpawnPoint());
                            Instance.UndoStack.Push(Classes.Editor.Solution.Entities.LastAction);
                            Instance.RedoStack.Clear();
                            UpdateControls();
                        })
                    };
                    Instance.ToolBarPanelRight.Children.Clear();
                    Instance.ToolBarPanelRight.Children.Add(Instance.EntitiesToolbar);
                    UpdateToolbars(true, true);
                    Instance.Editor_Resize(null, null);
                }
                UpdateEntitiesToolbarList();
                Instance.EntitiesToolbar.SelectedEntities = Classes.Editor.Solution.Entities.SelectedEntities.Select(x => x.Entity).ToList();
            }
            else
            {
                if (Instance.EntitiesToolbar != null)
                {
                    Instance.EntitiesToolbar.Dispose();
                    Instance.EntitiesToolbar = null;
                }
                if (Classes.Editor.Solution.Entities != null && Classes.Editor.Solution.Entities.SelectedEntities != null)
                {
                    if (Classes.Editor.Solution.Entities.SelectedEntities.Count != 0 && Classes.Editor.Solution.Entities.TemporarySelection.Count != 0)
                    {
                        Classes.Editor.Solution.Entities.EndTempSelection();
                        Classes.Editor.Solution.Entities.Deselect();
                    }
                }


            }
            if (Instance.TilesToolbar == null && Instance.EntitiesToolbar == null && (Instance.ToolBarPanelRight.Children.Count != 0))
            {
                Instance.ToolBarPanelRight.Children.Clear();
                UpdateToolbars(true, false);
                Instance.Editor_Resize(null, null);
            }

            SetSelectOnlyButtonsState(enabled);

            Position GetEntitySpawnPoint()
            {
                if (Instance.EditorToolbar.DrawToolButton.IsChecked.Value)
                {
                    short x = (short)(Classes.Editor.SolutionState.LastX / Classes.Editor.SolutionState.Zoom);
                    short y = (short)(Classes.Editor.SolutionState.LastY / Classes.Editor.SolutionState.Zoom);
                    if (Classes.Editor.SolutionState.UseMagnetMode)
                    {
                        short alignedX = (short)(Classes.Editor.SolutionState.MagnetSize * (x / Classes.Editor.SolutionState.MagnetSize));
                        short alignedY = (short)(Classes.Editor.SolutionState.MagnetSize * (y / Classes.Editor.SolutionState.MagnetSize));
                        return new Position(alignedX, alignedY);
                    }
                    else
                    {
                        return new Position(x, y);
                    }

                }
                else
                {
                    return new Position((short)(Classes.Editor.SolutionState.ViewPositionX / Classes.Editor.SolutionState.Zoom), (short)(Classes.Editor.SolutionState.ViewPositionY / Classes.Editor.SolutionState.Zoom));
                }

            }
        }
        public static void ToggleEditorButtons(bool enabled, bool isParallaxAnimation = false)
        {
            Instance.EditorMenuBar.MenuBar.IsEnabled = enabled;
            Instance.EditorToolbar.LayerToolbar.IsEnabled = enabled;
            Instance.EditorToolbar.MainToolbarButtons.IsEnabled = enabled;
            SetSceneOnlyButtonsState((enabled ? true : Classes.Editor.Solution.CurrentScene != null));
            Instance.EditorToolbar.LayerToolbar.IsEnabled = enabled;
            Instance.EditorStatusBar.StatusBar1.IsEnabled = enabled;
            Instance.EditorStatusBar.StatusBar2.IsEnabled = enabled;
            if (Instance.TilesToolbar != null) Instance.TilesToolbar.IsEnabled = enabled;
            if (Instance.EntitiesToolbar != null) Instance.EntitiesToolbar.IsEnabled = enabled;
            if (isParallaxAnimation)
            {
                Instance.EditorToolbar.LayerToolbar.IsEnabled = true;
                foreach (var pair in Instance.ExtraLayerEditViewButtons)
                {
                    pair.Key.IsEnabled = false;
                    pair.Value.IsEnabled = true;
                }
                Instance.EditorToolbar.EditFGHigh.IsEnabled = false;
                Instance.EditorToolbar.EditFGHigher.IsEnabled = false;
                Instance.EditorToolbar.EditFGLow.IsEnabled = false;
                Instance.EditorToolbar.EditFGLower.IsEnabled = false;

            }
        }
        public static void UpdateTilesOptions()
        {
            if (ManiacEditor.Classes.Editor.SolutionState.IsTilesEdit() && !ManiacEditor.Classes.Editor.SolutionState.IsChunksEdit())
            {
                if (Instance.TilesToolbar != null)
                {
                    List<ushort> values = Classes.Editor.Solution.EditLayerA?.GetSelectedValues();
                    List<ushort> valuesB = Classes.Editor.Solution.EditLayerB?.GetSelectedValues();
                    if (valuesB != null) values.AddRange(valuesB);

                    if (values.Count > 0)
                    {
                        for (int i = 0; i < 4; ++i)
                        {
                            bool set = ((values[0] & (1 << (i + 12))) != 0);
                            bool unk = false;
                            foreach (ushort value in values)
                            {
                                if (set != ((value & (1 << (i + 12))) != 0))
                                {
                                    unk = true;
                                    break;
                                }
                            }
                            Instance.TilesToolbar.SetTileOptionState(i, unk ? ManiacEditor.Controls.Base.Toolbars.TilesToolbar.TilesToolbar.TileOptionState.Indeterminate : set ? ManiacEditor.Controls.Base.Toolbars.TilesToolbar.TilesToolbar.TileOptionState.Checked : ManiacEditor.Controls.Base.Toolbars.TilesToolbar.TilesToolbar.TileOptionState.Unchcked);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 4; ++i)
                            Instance.TilesToolbar.SetTileOptionState(i, ManiacEditor.Controls.Base.Toolbars.TilesToolbar.TilesToolbar.TileOptionState.Disabled);
                    }
                }

            }
        }
        public static void UpdateEntitiesToolbarList()
        {
            Instance.EntitiesToolbar.Entities = Classes.Editor.Solution.Entities.Entities.Select(x => x.Entity).ToList();
        }
        public static void UpdateEditLayerActions()
        {
            if (Classes.Editor.Solution.EditLayerA != null)
            {
                List<IAction> actions = Classes.Editor.Solution.EditLayerA?.Actions;
                if (actions.Count > 0) Instance.RedoStack.Clear();
                while (actions.Count > 0)
                {
                    bool create_new = false;
                    if (Instance.UndoStack.Count == 0 || !(Instance.UndoStack.Peek() is ActionsGroup))
                    {
                        create_new = true;
                    }
                    else
                    {
                        create_new = (Instance.UndoStack.Peek() as ActionsGroup).IsClosed;
                    }
                    if (create_new)
                    {
                        Instance.UndoStack.Push(new ActionsGroup());
                    }
                    (Instance.UndoStack.Peek() as ActionsGroup).AddAction(actions[0]);
                    actions.RemoveAt(0);
                }
            }
            if (Classes.Editor.Solution.EditLayerB != null)
            {
                List<IAction> actions = Classes.Editor.Solution.EditLayerB?.Actions;
                if (actions.Count > 0) Instance.RedoStack.Clear();
                while (actions.Count > 0)
                {
                    bool create_new = false;
                    if (Instance.UndoStack.Count == 0 || !(Instance.UndoStack.Peek() is ActionsGroup))
                    {
                        create_new = true;
                    }
                    else
                    {
                        create_new = (Instance.UndoStack.Peek() as ActionsGroup).IsClosed;
                    }
                    if (create_new)
                    {
                        Instance.UndoStack.Push(new ActionsGroup());
                    }
                    (Instance.UndoStack.Peek() as ActionsGroup).AddAction(actions[0]);
                    actions.RemoveAt(0);
                }
            }
        }
        public static void UpdateToolbars(bool rightToolbar = true, bool visible = false, bool fullCollapse = false)
        {
            if (rightToolbar)
            {
                if (visible)
                {
                    Instance.ToolbarRight.Width = new GridLength(300);
                    Instance.ToolbarRight.MinWidth = 300;
                    Instance.ToolbarRight.MaxWidth = Instance.ViewPanelForm.ActualWidth / 3;
                    Instance.SplitterRight.Width = new GridLength(6);
                    Instance.SplitterRight.MinWidth = 6;
                }
                else
                {
                    Instance.ToolbarRight.Width = new GridLength(0);
                    Instance.ToolbarRight.MinWidth = 0;
                    Instance.ToolbarRight.MaxWidth = 0;
                    Instance.SplitterRight.Width = new GridLength(0);
                    Instance.SplitterRight.MinWidth = 0;
                }
            }

            else
            {
                if (visible)
                {
                    Instance.ToolbarLeft.Width = new GridLength(200);
                    Instance.ToolbarLeft.MinWidth = 200;
                    Instance.ToolbarLeft.MaxWidth = Instance.ViewPanelForm.ActualWidth / 3;
                    Instance.SplitterLeft.Width = new GridLength(3);
                    Instance.SplitterLeft.MinWidth = 3;
                    Instance.LeftToolbarToolbox.Visibility = Visibility.Visible;
                }
                else
                {
                    if (!fullCollapse)
                    {
                        Instance.ToolbarLeft.Width = new GridLength(10);
                        Instance.ToolbarLeft.MinWidth = 10;
                        Instance.ToolbarLeft.MaxWidth = 10;
                        Instance.SplitterLeft.Width = new GridLength(0);
                        Instance.SplitterLeft.MinWidth = 0;
                        Instance.LeftToolbarToolbox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Instance.ToolbarLeft.Width = new GridLength(0);
                        Instance.ToolbarLeft.MinWidth = 0;
                        Instance.ToolbarLeft.MaxWidth = 0;
                        Instance.SplitterLeft.Width = new GridLength(0);
                        Instance.SplitterLeft.MinWidth = 0;
                        Instance.LeftToolbarToolbox.Visibility = Visibility.Hidden;
                    }

                }
            }

        }
        public static void UpdateWaitingScreen(bool show)
        {
            if (show)
            {
                Instance.ViewPanelForm.Visibility = Visibility.Hidden;
                Instance.WaitingPanel.Visibility = Visibility.Visible;
            }
            else
            {
                Instance.ViewPanelForm.Visibility = Visibility.Visible;
                Instance.WaitingPanel.Visibility = Visibility.Collapsed;
            }

        }
        public static void UpdateSplineSpawnObjectsList(List<RSDKv5.SceneObject> sceneObjects)
        {
            Classes.Editor.SolutionState.AllowSplineOptionsUpdate = false;
            sceneObjects.Sort((x, y) => x.Name.ToString().CompareTo(y.Name.ToString()));
            var bindingSceneObjectsList = new System.ComponentModel.BindingList<RSDKv5.SceneObject>(sceneObjects);


            Instance.SplineSelectedObjectSpawnList.Clear();
            foreach (var _object in bindingSceneObjectsList)
            {
                TextBlock item = new TextBlock()
                {
                    Tag = _object,
                    Text = _object.Name.Name
                };
                Instance.SplineSelectedObjectSpawnList.Add(item);
            }

            if (Instance.SplineSelectedObjectSpawnList != null && Instance.SplineSelectedObjectSpawnList.Count > 1)
            {
                Instance.EditorToolbar.SelectedSplineRender.ItemsSource = Instance.SplineSelectedObjectSpawnList;
                Instance.EditorToolbar.SelectedSplineRender.SelectedItem = Instance.EditorToolbar.SelectedSplineRender.Items[0];
                var SelectedItem = Instance.EditorToolbar.SelectedSplineRender.SelectedItem as TextBlock;
                if (SelectedItem == null) return;
                SelectedItem.Foreground = (System.Windows.Media.SolidColorBrush)Instance.FindResource("NormalText");
                Classes.Editor.SolutionState.AllowSplineOptionsUpdate = true;

            }
        }
        public static void UpdateSplineSettings(int splineID)
        {
            if (!Classes.Editor.SolutionState.SplineOptionsGroup.ContainsKey(splineID)) Classes.Editor.SolutionState.SplineOptionsGroup.Add(splineID, new Classes.Editor.SolutionState.SplineOptions());
            Instance.EditorToolbar.SplineLineMode.IsChecked = Classes.Editor.SolutionState.SplineOptionsGroup[splineID].SplineLineMode;
            Instance.EditorToolbar.SplineOvalMode.IsChecked = Classes.Editor.SolutionState.SplineOptionsGroup[splineID].SplineOvalMode;
            Instance.EditorToolbar.SplineShowLineCheckbox.IsChecked = Classes.Editor.SolutionState.SplineOptionsGroup[splineID].SplineToolShowLines;
            Instance.EditorToolbar.SplineShowObjectsCheckbox.IsChecked = Classes.Editor.SolutionState.SplineOptionsGroup[splineID].SplineToolShowObject;
            Instance.EditorToolbar.SplineShowPointsCheckbox.IsChecked = Classes.Editor.SolutionState.SplineOptionsGroup[splineID].SplineToolShowPoints;
            Instance.EditorToolbar.SplinePointSeperationNUD.Value = Classes.Editor.SolutionState.SplineOptionsGroup[splineID].SplineSize;
            Instance.EditorToolbar.SplinePointSeperationSlider.Value = Classes.Editor.SolutionState.SplineOptionsGroup[splineID].SplineSize;

            if (Classes.Editor.SolutionState.SplineOptionsGroup[splineID].SplineObjectRenderingTemplate != null)
                Instance.EditorToolbar.SplineRenderObjectName.Content = Classes.Editor.SolutionState.SplineOptionsGroup[splineID].SplineObjectRenderingTemplate.Entity.Object.Name.Name;
            else
                Instance.EditorToolbar.SplineRenderObjectName.Content = "None";
        }
        public static void UpdateSplineToolbox()
        {
            //Editor.Instance.SplineInfoLabel1.Text = string.Format("Number of Spline Objects: {0}", Editor.Instance.UIModes.SplineTotalNumberOfObjects);
            //Editor.Instance.SplineInfoLabel2.Text = string.Format("Point Frequency: {0}", Editor.Instance.UIModes.SplineSize);
            //Editor.Instance.SplineInfoLabel3.Text = string.Format("Total Number of Rendered Points: {0}", Editor.Instance.UIModes.SplineCurrentPointsDrawn);
        }
        public static void UpdateCustomColors()
        {
            Instance.EditorToolbar.CSAC.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(Classes.Editor.SolutionState.CollisionSAColour.A, Classes.Editor.SolutionState.CollisionSAColour.R, Classes.Editor.SolutionState.CollisionSAColour.G, Classes.Editor.SolutionState.CollisionSAColour.B));
            Instance.EditorToolbar.SSTOC.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(Classes.Editor.SolutionState.CollisionTOColour.A, Classes.Editor.SolutionState.CollisionTOColour.R, Classes.Editor.SolutionState.CollisionTOColour.G, Classes.Editor.SolutionState.CollisionTOColour.B));
            Instance.EditorToolbar.CSLRDC.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(Classes.Editor.SolutionState.CollisionLRDColour.A, Classes.Editor.SolutionState.CollisionLRDColour.R, Classes.Editor.SolutionState.CollisionLRDColour.G, Classes.Editor.SolutionState.CollisionLRDColour.B));
            Instance.EditorToolbar.WLC.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(Classes.Editor.SolutionState.waterColor.A, Classes.Editor.SolutionState.waterColor.R, Classes.Editor.SolutionState.waterColor.G, Classes.Editor.SolutionState.waterColor.B));
            Instance.EditorToolbar.GDC.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(Classes.Editor.SolutionState.GridColor.A, Classes.Editor.SolutionState.GridColor.R, Classes.Editor.SolutionState.GridColor.G, Classes.Editor.SolutionState.GridColor.B));
        }
        public static void UpdateControls(bool stageLoad = false)
        {
            if (Instance != null)
            {
                if (Core.Settings.MySettings.EntityFreeCam)
                {
                    Instance.DeviceModel.vScrollBar1.IsEnabled = false;
                    Instance.DeviceModel.hScrollBar1.IsEnabled = false;
                }
                else
                {
                    Instance.DeviceModel.vScrollBar1.IsEnabled = true;
                    Instance.DeviceModel.hScrollBar1.IsEnabled = true;
                }

                bool parallaxAnimationInProgress = Classes.Editor.SolutionState.AllowAnimations && Classes.Editor.SolutionState.ParallaxAnimationChecked;

                Instance.EditorToolbar.UpdateGameRunningButton(Classes.Editor.Solution.CurrentScene != null);
                Methods.Internal.Theming.UpdateThemeForItemsWaiting();
                Instance.EditorStatusBar.UpdateFilterButtonApperance(false);
                Instance.EditorStatusBar.UpdateStatusPanel();
                SetSceneOnlyButtonsState(Classes.Editor.Solution.CurrentScene != null && !parallaxAnimationInProgress, stageLoad);
                SetParallaxAnimationOnlyButtonsState(parallaxAnimationInProgress);
                UpdateSplineToolbox();
                Instance.EditorToolbar.CustomGridLabel.Text = string.Format(Instance.EditorToolbar.CustomGridLabel.Tag.ToString(), Properties.Defaults.Default.CustomGridSizeValue);
            }
        }
        private static void UpdateTooltips()
        {
            UpdateTooltipForStacks(Instance.EditorToolbar.UndoButton, Instance.UndoStack);
            UpdateTooltipForStacks(Instance.EditorToolbar.RedoButton, Instance.RedoStack);
            UpdateTextBlockForStacks(Instance.EditorMenuBar.UndoMenuItemInfo, Instance.UndoStack);
            UpdateTextBlockForStacks(Instance.EditorMenuBar.RedoMenuItemInfo, Instance.RedoStack);
            if (Instance.EditorControls != null)
            {
                if (Instance.IsVisible)
                {
                    Instance.EditorMenuBar.UpdateMenuItems();
                    Instance.EditorControls.UpdateTooltips();
                }

            }

        }
        private static void UpdateTextBlockForStacks(TextBlock tsb, Stack<IAction> actionStack)
        {
            if (actionStack?.Count > 0)
            {
                IAction action = actionStack.Peek();
                tsb.Visibility = Visibility.Visible;
                tsb.Text = string.Format("({0})", action.Description);
            }
            else
            {
                tsb.Visibility = Visibility.Collapsed;
                tsb.Text = string.Empty;
            }
        }
        private static void UpdateTooltipForStacks(Button tsb, Stack<IAction> actionStack)
        {
            if (actionStack?.Count > 0)
            {
                IAction action = actionStack.Peek();
                System.Windows.Controls.ToolTip tooltip = new System.Windows.Controls.ToolTip { Content = string.Format(tsb.Tag.ToString(), action.Description + " ") };
                tsb.ToolTip = tooltip;
            }
            else
            {
                System.Windows.Controls.ToolTip tooltip = new System.Windows.Controls.ToolTip { Content = string.Format(tsb.Tag.ToString(), string.Empty) };
                tsb.ToolTip = tooltip;
            }
        }
        public static void ReloadSpritesAndTextures()
        {
            try
            {
                // release all our resources, and force a reload of the tiles
                // Entities should take care of themselves
                Instance.DisposeTextures();
                Instance.EntityDrawing.ReleaseResources();
                //EditorEntity_ini.rendersWithErrors.Clear();

                //Reload for Encore Palletes, otherwise reload the image normally
                if (Classes.Editor.SolutionState.UseEncoreColors == true)
                {
                    Classes.Editor.Solution.CurrentTiles?.Image.Reload(Instance.EncorePalette[0]);
                    Instance.TilesToolbar?.Reload(Instance.EncorePalette[0]);
                }
                else
                {
                    Classes.Editor.Solution.CurrentTiles?.Image.Reload();
                    Instance.TilesToolbar?.Reload();
                }

                Classes.Editor.Solution.TileConfig = new Tileconfig(Instance.Paths.TileConfig_Source);



            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
    }
}
