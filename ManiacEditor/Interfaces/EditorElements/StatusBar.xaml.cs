﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Specialized;

namespace ManiacEditor.Interfaces.EditorElements
{
    /// <summary>
    /// Interaction logic for StatusBar.xaml
    /// </summary>
    public partial class StatusBar : UserControl
    {
        public StatusBar()
        {
            InitializeComponent();
        }

        public void UpdatePositionLabel(System.Windows.Forms.MouseEventArgs e)
        {

            if (Editor.Instance.Options.CountTilesSelectedInPixels == false)
            {
                positionLabel.Content = "X: " + (int)(e.X / EditorStateModel.Zoom) + " Y: " + (int)(e.Y / EditorStateModel.Zoom);
            }
            else
            {
                positionLabel.Content = "X: " + (int)((e.X / EditorStateModel.Zoom) / 16) + " Y: " + (int)((e.Y / EditorStateModel.Zoom) / 16);
            }
        }

        public void UpdateStatusPanel()
        {
            //
            // Tooltip Bar Info 
            //

            _levelIDLabel.Content = "Level ID: " + Editor.Instance.Options.LevelID.ToString();
            seperator1.Visibility = Visibility.Visible;
            seperator2.Visibility = Visibility.Visible;
            seperator3.Visibility = Visibility.Visible;
            seperator4.Visibility = Visibility.Visible;
            seperator5.Visibility = Visibility.Visible;
            seperator6.Visibility = Visibility.Visible;
            seperator7.Visibility = Visibility.Visible;
            //seperator8.Visibility = Visibility.Visible;
            //seperator9.Visibility = Visibility.Visible;

            if (Editor.Instance.Options.CountTilesSelectedInPixels == false)
            {
                selectedPositionLabel.Content = "Selected Tile Position: X: " + (int)EditorStateModel.SelectedTileX + ", Y: " + (int)EditorStateModel.SelectedTileY;
                selectedPositionLabel.ToolTip = "The Position of the Selected Tile";
            }
            else
            {
                selectedPositionLabel.Content = "Selected Tile Pixel Position: " + "X: " + (int)EditorStateModel.SelectedTileX * 16 + ", Y: " + (int)EditorStateModel.SelectedTileY * 16;
                selectedPositionLabel.ToolTip = "The Pixel Position of the Selected Tile";
            }
            if (Editor.Instance.Options.CountTilesSelectedInPixels == false)
            {
                selectionSizeLabel.Content = "Amount of Tiles in Selection: " + (EditorStateModel.SelectedTilesCount - EditorStateModel.DeselectTilesCount);
                selectionSizeLabel.ToolTip = "The Size of the Selection";
            }
            else
            {
                selectionSizeLabel.Content = "Length of Pixels in Selection: " + (EditorStateModel.SelectedTilesCount - EditorStateModel.DeselectTilesCount) * 16;
                selectionSizeLabel.ToolTip = "The Length of all the Tiles (by Pixels) in the Selection";
            }

            selectionBoxSizeLabel.Content = "Selection Box Size: X: " + (EditorStateModel.select_x2 - EditorStateModel.select_x1) + ", Y: " + (EditorStateModel.select_y2 - EditorStateModel.select_y1);

            scrollLockDirLabel.Content = "Scroll Direction: " + (Editor.Instance.Options.ScrollDirection == (int)ScrollDir.X ? "X" : "Y") + (Editor.Instance.Options.ScrollLocked ? " (Locked)" : "");


            hVScrollBarXYLabel.Content = "Zoom Value: " + EditorStateModel.Zoom.ToString();

            //
            // End of Tooltip Bar Info Section
            //
        }

        public void QuickButtonClickEvent(object sender, RoutedEventArgs e)
        {
            if (sender == MoreSettingsButton)
            {
                switch (Editor.Instance.Options.LastQuickButtonState)
                {
                    case 1:
                        Editor.Instance.UIEvents.SetScrollLockDirection();
                        break;
                    case 2:
                        Editor.Instance.Options.ApplyEditEntitiesTransparency ^= true;
                        break;
                    case 3:
                        Editor.Instance.UIEvents.SwapEncoreManiaEntityVisibility();
                        break;
                    default:
                        Editor.Instance.Options.LastQuickButtonState = 1;
                        Editor.Instance.UIEvents.SetScrollLockDirection();
                        break;
                }
            }
            else if (sender == QuickSwapScrollDirection)
            {
                Editor.Instance.Options.LastQuickButtonState = 1;
                Editor.Instance.UIEvents.SetScrollLockDirection();
            }
            else if (sender == QuickSwapEncoreManiaEntitVisibility)
            {
                Editor.Instance.Options.LastQuickButtonState = 3;
                Editor.Instance.UIEvents.SwapEncoreManiaEntityVisibility();
            }
            else if (sender == QuickEditEntitiesTransparentLayers)
            {
                Editor.Instance.Options.LastQuickButtonState = 2;
                Editor.Instance.Options.ApplyEditEntitiesTransparency ^= true;
            }

        }
        private void FilterButtonOpenContextMenuEvent(object sender, RoutedEventArgs e) { FilterButton.ContextMenu.IsOpen = true; }
        private void FilterCheckChangedEvent(object sender, RoutedEventArgs e)
        {
            if (Classes.Edit.Scene.Solution.Entities != null) Classes.Edit.Scene.Solution.Entities.FilterRefreshNeeded = true;
        }

        public void UpdateTooltips()
        {
            positionLabel.ToolTip = "The position relative to your mouse (Pixels Only for Now)";
            selectionSizeLabel.ToolTip = "The Size of the Selection";
            selectedPositionLabel.ToolTip = "The Position of the Selected Tile";
            selectionBoxSizeLabel.ToolTip = "The Size of the Selection Box";
            pixelModeButton.ToolTip = "Change the Positional/Selection Values to Pixel or Tile Based Values";
            nudgeFasterButton.ToolTip = "Move entities/tiles in a larger increment. (Configurable in Options)\r\nShortcut Key: " + KeyBindPraser("NudgeFaster");
            scrollLockButton.ToolTip = "Prevent the Mouse Wheel from Scrolling with the vertical scroll bar\r\nShortcut Key: " + KeyBindPraser("ScrollLock");
            QuickSwapScrollDirection.InputGestureText = KeyBindPraser("ScrollLockTypeSwitch", false, true);
        }

        public string KeyBindPraser(string keyRefrence, bool tooltip = false, bool nonRequiredBinding = false)
        {
            string nullString = (nonRequiredBinding ? "" : "N/A");
            if (nonRequiredBinding && tooltip) nullString = "None";
            List<string> keyBindList = new List<string>();
            List<string> keyBindModList = new List<string>();

            if (!Extensions.KeyBindsSettingExists(keyRefrence)) return nullString;

            if (Properties.KeyBinds.Default == null) return nullString;

            var keybindDict = Properties.KeyBinds.Default[keyRefrence] as StringCollection;
            if (keybindDict != null)
            {
                keyBindList = keybindDict.Cast<string>().ToList();
            }
            else
            {
                return nullString;
            }

            if (keyBindList == null)
            {
                return nullString;
            }

            if (keyBindList.Count > 1)
            {
                string keyBindLister = "";
                foreach (string key in keyBindList)
                {
                    keyBindLister += String.Format("({0}) ", key);
                }
                if (tooltip) return String.Format(" ({0})", keyBindLister);
                else return keyBindLister;
            }
            else if ((keyBindList.Count == 1) && keyBindList[0] != "None")
            {
                if (tooltip) return String.Format(" ({0})", keyBindList[0]);
                else return keyBindList[0];
            }
            else
            {
                return nullString;
            }


        }

        public void UpdateFilterButtonApperance(bool startup)
        {
            if (startup)
            {
                maniaFilterCheck.Foreground = Editor.Instance.Theming.GetColorBrush(2);
                encoreFilterCheck.Foreground = Editor.Instance.Theming.GetColorBrush(4);
                otherFilterCheck.Foreground = Editor.Instance.Theming.GetColorBrush(0);
                bothFilterCheck.Foreground = Editor.Instance.Theming.GetColorBrush(1);
                pinballFilterCheck.Foreground = Editor.Instance.Theming.GetColorBrush(255);
            }
            if (Properties.Settings.Default.UseBitOperators)
            {
                maniaFilterCheck.Content = "Mania (0b0010)";
                encoreFilterCheck.Content = "Encore (0b0100)";
                otherFilterCheck.Content = "Other (0b0000)";
                bothFilterCheck.Content = "Both (0b0001)";
                pinballFilterCheck.Content = "All (0b11111111)";
            }
            else
            {
                maniaFilterCheck.Content = "Mania (2)";
                encoreFilterCheck.Content = "Encore (4)";
                otherFilterCheck.Content = "Other (0)";
                bothFilterCheck.Content = "Both (1 & 5)";
                pinballFilterCheck.Content = "All (255)";
            }
        }

        private void TileManiacEditTileEvent(object sender, RoutedEventArgs e) { EditorLaunch.TileManiacIntergration(); }
        private void TogglePixelModeEvent(object sender, RoutedEventArgs e) { Editor.Instance.Options.CountTilesSelectedInPixels ^= true; }
        public void ToggleScrollLockEvent(object sender, RoutedEventArgs e) { Editor.Instance.Options.ScrollLocked ^= true; }
        public void ToggleFasterNudgeEvent(object sender, RoutedEventArgs e) { Editor.Instance.Options.EnableFasterNudge ^= true; }
    }
}
