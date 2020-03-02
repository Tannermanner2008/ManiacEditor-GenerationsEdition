﻿using System;
using System.Windows;
using System.Windows.Controls;
using RSDKv5;
using System.Diagnostics;

namespace ManiacEditor.Controls.Editor.Toolbars.TilesToolbar
{
	/// <summary>
	/// Interaction logic for TilesToolbar.xaml
	/// </summary>
	public partial class TilesToolbar : UserControl
	{
		public ManiacEditor.Controls.Editor.MainEditor EditorInstance;

		bool disposing = false;
		ManiacEditor.Methods.Draw.GIF TileGridImage;
		string TilesImagePath;
		public Action<ushort> TileDoubleClick { get; set; } 

		bool TilesFlipHorizontal = false;
		bool TilesFlipVertical = false;

		public System.Windows.Forms.Panel tilePanel;
		public string CurrentColorPalette;

		public ManiacEditor.Controls.Global.Controls.RetroEDTileList ChunkList;
        public ManiacEditor.Controls.Global.Controls.RetroEDTileList TilesList;

		public System.Windows.Forms.Integration.WindowsFormsHost host;
		public System.Windows.Forms.Integration.WindowsFormsHost host3;

		public Action<int, bool> TileOptionChanged;

		CheckBox[] CurrentTileCheckboxOptions = new CheckBox[6];
		CheckBox[] SelectedTilesCheckboxOptions = new CheckBox[4];
		bool setCheckboxes;

		public int SelectedTileIndex
		{
			get
			{
				return TilesList.SelectedIndex;
			}
		}

		public ushort SelectedTile
		{
			get
			{
				ushort Tile = (ushort)TilesList.SelectedIndex;

				bool FlipX = CurrentTileCheckboxOptions[0].IsChecked.Value;
				bool FlipY = CurrentTileCheckboxOptions[1].IsChecked.Value;
				bool SolidTopA = CurrentTileCheckboxOptions[2].IsChecked.Value;
				bool SolidLrbA = CurrentTileCheckboxOptions[3].IsChecked.Value;
				bool SolidTopB = CurrentTileCheckboxOptions[4].IsChecked.Value;
				bool SolidLrbB = CurrentTileCheckboxOptions[5].IsChecked.Value;

				Tile = (ushort)ManiacEditor.Methods.Layers.TileFindReplace.SetBit(10, FlipX, Tile);
				Tile = (ushort)ManiacEditor.Methods.Layers.TileFindReplace.SetBit(11, FlipY, Tile);
				Tile = (ushort)ManiacEditor.Methods.Layers.TileFindReplace.SetBit(12, SolidTopA, Tile);
				Tile = (ushort)ManiacEditor.Methods.Layers.TileFindReplace.SetBit(13, SolidLrbA, Tile);
				Tile = (ushort)ManiacEditor.Methods.Layers.TileFindReplace.SetBit(14, SolidTopB, Tile);
				Tile = (ushort)ManiacEditor.Methods.Layers.TileFindReplace.SetBit(15, SolidLrbB, Tile);

				return Tile;
			}
		}

		public bool ShowShortcuts { set { UpdateShortcuts(value); } }

		public enum TileOptionState
		{
			Disabled,
			Checked,
			Unchcked,
			Indeterminate
		}

		public void SetSelectTileOption(int option, bool state)
		{
			CurrentTileCheckboxOptions[option].IsChecked = state;
		}

		public void SetTileOptionState(int option, TileOptionState state)
		{
			setCheckboxes = true;
			switch (state)
			{
				case TileOptionState.Disabled:
					SelectedTilesCheckboxOptions[option].IsEnabled = false;
					SelectedTilesCheckboxOptions[option].IsChecked = false;
					break;
				case TileOptionState.Checked:
					SelectedTilesCheckboxOptions[option].IsEnabled = true;
					SelectedTilesCheckboxOptions[option].IsChecked = true;
					break;
				case TileOptionState.Unchcked:
					SelectedTilesCheckboxOptions[option].IsEnabled = true;
					SelectedTilesCheckboxOptions[option].IsChecked = false;
					break;
				case TileOptionState.Indeterminate:
					SelectedTilesCheckboxOptions[option].IsEnabled = true;
					SelectedTilesCheckboxOptions[option].IsChecked = null;
					break;
			}
			setCheckboxes = false;
		}

		public TilesToolbar(Classes.Scene.EditorTiles tiles, String data_directory, String Colors, ManiacEditor.Controls.Editor.MainEditor instance)
		{
            try
            {
                InitializeComponent();
                SetupTilesList(instance);

                EditorInstance = instance;

                SelectedTilesCheckboxOptions[0] = tileOption1;
                SelectedTilesCheckboxOptions[1] = tileOption2;
                SelectedTilesCheckboxOptions[2] = tileOption3;
                SelectedTilesCheckboxOptions[3] = tileOption4;

                CurrentTileCheckboxOptions[0] = option1CheckBox;
                CurrentTileCheckboxOptions[1] = option2CheckBox;
                CurrentTileCheckboxOptions[2] = option3CheckBox;
                CurrentTileCheckboxOptions[3] = option4CheckBox;
                CurrentTileCheckboxOptions[4] = option5CheckBox;
                CurrentTileCheckboxOptions[5] = option6CheckBox;

                TilesImagePath = data_directory + "\\16x16Tiles.gif";
                TileGridImage = new Methods.Draw.GIF((TilesImagePath), Colors);

                UpdateShortcuts();

                ChunksReload();
                TilesReload(Colors);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }

		}

        public void UpdateThemeColors()
        {
            System.Drawing.Color ListBackColor = (App.Skin == Skin.Dark ? Methods.Internal.Theming.DarkTheme0 : System.Drawing.Color.White);
            this.ChunkList.BackColor = ListBackColor;
            this.tilePanel.BackColor = ListBackColor;
            this.TilesList.BackColor = ListBackColor;
        }

        public void SetupTilesList(ManiacEditor.Controls.Editor.MainEditor instance)
		{
			host = new System.Windows.Forms.Integration.WindowsFormsHost();
			host3 = new System.Windows.Forms.Integration.WindowsFormsHost();
			this.ChunkList = new ManiacEditor.Controls.Global.Controls.RetroEDTileList();

			this.tilePanel = new System.Windows.Forms.Panel();



            // 
            // ChunkList
            // 
            this.ChunkList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChunkList.ImageHeight = 16 * 8;
			this.ChunkList.ImageSize = 16 * 8;
			this.ChunkList.ImageWidth = 16 * 8;
			this.ChunkList.Location = new System.Drawing.Point(3, 3);
			this.ChunkList.Name = "ChunkList";
			this.ChunkList.ScrollValue = 0;
			this.ChunkList.SelectedIndex = -1;
			this.ChunkList.Size = new System.Drawing.Size(234, 290);
			this.ChunkList.TabIndex = 1;
			this.ChunkList.MouseClick += ChunkList_Click;
			this.ChunkList.SelectedIndexChanged += ChunkList_SelectedIndexChanged;

			// 
			// TileList
			//
			this.TilesList = new ManiacEditor.Controls.Global.Controls.RetroEDTileList();
            this.TilesList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TilesList.ImageHeight = 16 * 1;
			this.TilesList.ImageSize = 16 * 1;
			this.TilesList.ImageWidth = 16 * 1;
			this.TilesList.Location = new System.Drawing.Point(3, 3);
			this.TilesList.Name = "TilesList";
			this.TilesList.ScrollValue = 0;
			this.TilesList.SelectedIndex = -1;
			this.TilesList.Size = new System.Drawing.Size(234, 290);
			this.TilesList.TabIndex = 1;
			this.TilesList.MouseClick += TilesList_MouseClick; ;
			this.TilesList.SelectedIndexChanged += TilesListList_SelectedIndexChanged;
			this.TilesList.MouseMove += TilesList_MouseMove;
			this.TilesList.MouseDoubleClick += TilesList_MouseDoubleClick;
			this.tilePanel.Controls.Add(this.TilesList);

			// 
			// tilePanel
			// 
			this.tilePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
            this.tilePanel.Location = new System.Drawing.Point(0, 45);
			this.tilePanel.Name = "tilePanel";
			this.tilePanel.Size = new System.Drawing.Size(241, 253);
			this.tilePanel.TabIndex = 2;
			this.tilePanel.Dock = System.Windows.Forms.DockStyle.Fill;

            UpdateThemeColors();


            host.Child = tilePanel;
			host3.Child = ChunkList;
			TileViewer.Children.Add(host);
			ChunksPage.Children.Add(host3);
		}

		private void TilesList_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (disposing) return;
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				if (SelectedTileIndex != -1 && TileDoubleClick != null)
				{
					TileDoubleClick(SelectedTile);
				}
			}
		}

		private void TilesList_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			TilesList.editCollisionToolStripMenuItem.Enabled = false;
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				if (TilesList.SelectedIndex != -1)
				{
					TilesList.editCollisionToolStripMenuItem.Enabled = true;
					TilesList.editCollisionToolStripMenuItem.Text = string.Format("Edit Collision of Tile {0} with Tile Maniac", SelectedTileIndex);

					TilesList.Tiles16ToolStrip.Show(TilesList, e.Location);
				}
				else
				{
					TilesList.editCollisionToolStripMenuItem.Enabled = false;
					TilesList.editCollisionToolStripMenuItem.Text = string.Format("Edit Collision of Tile {0} with Tile Maniac", "N/A");

					TilesList.Tiles16ToolStrip.Show(TilesList, e.Location);
				}
			}
		}

		private void TileListDoubleClick()
		{

		}

		private void TilesList_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (disposing) return;
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				if (SelectedTileIndex != -1)
				{
					Int32 val = SelectedTileIndex;
					TilesList.DoDragDrop(val, System.Windows.Forms.DragDropEffects.Move);
				}
			}
		}

		private void TilePanel_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			Methods.Internal.Controls.GraphicPanel_OnKeyDown(sender, e);
		}

		private void TilePanel_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			Methods.Internal.Controls.GraphicPanel_OnKeyUp(sender, e);
		}

		private void ChunkList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ManiacEditor.Methods.Editor.SolutionState.IsChunksEdit())
			{
				EditorInstance.TilesToolbar.SelectedTileLabel.Content = "Selected Chunk: " + ChunkList.SelectedIndex.ToString();

			}
		}

		private void TilesListList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ManiacEditor.Methods.Editor.SolutionState.IsTilesEdit())
			{
				EditorInstance.TilesToolbar.SelectedTileLabel.Content = "Selected Tile: " + TilesList.SelectedIndex.ToString();
			}
		}

		private void ChunkList_Click(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ChunkList.moveChunkDownToolStripMenuItem.Enabled = false;
			ChunkList.moveChunkUpToolStripMenuItem.Enabled = false;
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
			{
				if (ChunkList.SelectedIndex != -1)
				{
					ChunkList.removeChunkToolStripMenuItem.Enabled = true;
					ChunkList.duplicateChunkToolStripMenuItem.Enabled = true;

					ChunkList.Chunks128ToolStrip.Show(ChunkList, e.Location);
				}
				else
				{
					ChunkList.removeChunkToolStripMenuItem.Enabled = false;
					ChunkList.duplicateChunkToolStripMenuItem.Enabled = false;

					ChunkList.Chunks128ToolStrip.Show(ChunkList, e.Location);
				}
			}
		}

		public void RemoveChunk(int index)
		{
			EditorInstance.Chunks.StageStamps.StampList.RemoveAt(index);
			ChunkList.Images.Clear();
			EditorInstance.Chunks.DisposeTextures();
			if (index != 0) ChunkList.SelectedIndex = index--;
			else ChunkList.SelectedIndex = -1;



			ChunksReload();
		}

		public void DuplicateChunk(int index)
		{
			EditorInstance.Chunks.StageStamps.StampList.Add(EditorInstance.Chunks.StageStamps.StampList[index]);

			ChunksReload();
		}

		private void trackBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (disposing) return;
			int scale = 16 << (int)trackBar1.Value;
			TilesList.ImageSize = scale;
		}

		public void Reload(string colors = null)
		{
			if (disposing) return;
			TilesReload(colors);
			ChunksReload();
		}

		public void TilesReload(string colors = "")
		{
			if (colors != "") CurrentColorPalette = colors;
			else if (CurrentColorPalette != null) colors = CurrentColorPalette;

			if (disposing) return;
			TilesList.Images.Clear();
			TileGridImage = new Methods.Draw.GIF((TilesImagePath), colors);

			for (int i = 0; i < 1024; i++)
			{
				TilesList.Images.Add(TileGridImage.GetBitmap(new System.Drawing.Rectangle(0,16*i,16,16) , TilesFlipHorizontal, TilesFlipVertical));
			}
			int indexStorage = TilesList.SelectedIndex;
			TilesList.SelectedIndex = -1;
			TilesList.SelectedIndex = indexStorage;
			TilesList.Refresh();
		}

		public void ChunksReload()
		{
			if (disposing) return;
			ChunkList.Images.Clear();
			if (EditorInstance.Chunks != null)
			{
				EditorInstance.Chunks.DisposeTextures();

				int indexStorage = ChunkList.SelectedIndex;
				ChunkList.SelectedIndex = -1;
				if (EditorInstance.Chunks.StageStamps != null)
				{
					for (int i = 0; i < EditorInstance.Chunks.StageStamps.StampList.Count; i++)
					{
						ChunkList.Images.Add(EditorInstance.Chunks.GetChunkTexture(i));
					}
					ChunkList.SelectedIndex = indexStorage;
				}
				ChunkList.Refresh();
			}

		}

		public void Dispose()
		{
			disposing = true;
			if (tilePanel != null)
			{
				tilePanel.Controls.Clear();
				tilePanel.Dispose();
				tilePanel = null;
			}
			if (TilesList != null)
			{
				TilesList.Controls.Clear();
				TilesList.Dispose();
				TilesList = null;
			}
			if (ChunkList != null)
			{
				ChunkList.Controls.Clear();
				ChunkList.Dispose();
				ChunkList = null;
			}
			if (host != null)
			{
				host.Child.Dispose();
				host = null;
			}
			if (host3 != null)
			{
				host3.Child.Dispose();
				host3 = null;
			}


		}

		private void TilesToolbar_Load(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("Send to debug output.");
			trackBar1.Value = Properties.Settings.MyDefaults.TilesDefaultZoom;
            trackBar2.Value = Properties.Settings.MyDefaults.ChunksDefaultZoom;

            option3CheckBox.IsChecked = Properties.Settings.MyDefaults.SolidTopADefault;
            option4CheckBox.IsChecked = Properties.Settings.MyDefaults.SolidAllButTopADefault;
            option5CheckBox.IsChecked = Properties.Settings.MyDefaults.SolidTopBDefault;
            option6CheckBox.IsChecked = Properties.Settings.MyDefaults.SolidAllButTopBDefault;

        }

		private void option1CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
		{
			TilesFlipHorizontal = option1CheckBox.IsChecked.Value;
			TilesReload();
		}

		private void option2CheckBox_CheckedChanged(object sender, RoutedEventArgs e)
		{
			TilesFlipVertical = option2CheckBox.IsChecked.Value;
			TilesReload();
		}

		private void tileOption1_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (!setCheckboxes) TileOptionChanged?.Invoke(0, tileOption1.IsChecked.Value);
		}

		private void tileOption2_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (!setCheckboxes) TileOptionChanged?.Invoke(1, tileOption2.IsChecked.Value);
		}

		private void tileOption3_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (!setCheckboxes) TileOptionChanged?.Invoke(2, tileOption3.IsChecked.Value);
		}

		private void tileOption4_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (!setCheckboxes) TileOptionChanged?.Invoke(3, tileOption4.IsChecked.Value);
		}

		private void UpdateShortcuts(bool show = false)
		{
			if (show)
			{
				option1CheckBox.Content = "Flip Horizontal " + Environment.NewLine + Extensions.KeyEventExts.KeyBindPraser("FlipHTiles", true);
				option2CheckBox.Content = "Flip Vertical " + Environment.NewLine + Extensions.KeyEventExts.KeyBindPraser("FlipVTiles", true);
			}
			else
			{
				option1CheckBox.Content = "Flip Horizontal" + Environment.NewLine + string.Format("({0} - Selected Only)", Extensions.KeyEventExts.KeyBindPraser("FlipH"));
				option2CheckBox.Content = "Flip Vertical" + Environment.NewLine + string.Format("({0} - Selected Only)", Extensions.KeyEventExts.KeyBindPraser("FlipV"));
			}
		}

		private void tabControl1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.Source is TabControl)
			{
				if (TabControl.SelectedIndex == 0) EditorInstance.EditorToolbar.ChunksToolButton.IsChecked = false;
				else
				{
					EditorInstance.EditorToolbar.ChunksToolButton.IsChecked = true;
					EditorInstance.TilesToolbar.SelectedTileLabel.Content = "Selected Chunk: " + ChunkList.SelectedIndex.ToString();
				}
			}
			if (TabControl.SelectedIndex == 0)
			{
				TilesReload();
				EditorInstance.TilesToolbar.SelectedTileLabel.Content = "Selected Tile: " + TilesList.SelectedIndex.ToString();
			}

		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e)
		{
			//this.Dispose();
		}

		private void TabItem_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (TabControl.SelectedIndex == 0)
			{
				//tilesList.graphicPanel.Render();
			}
		}

		private void SaveChunksManually_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (EditorInstance.Chunks.StageStamps?.loadstate == Stamps.LoadState.Upgrade)
				{
					MessageBoxResult result = MessageBox.Show("This Editor Chunk File needs to be updated to a newer version of the format. This will happen almost instantly, however you will be unable to use your chunks in a previous version of maniac on this is done. Would you like to continue?" + Environment.NewLine + "(Click Yes to Save, Click No to Continue without Saving Your Chunks)", "Chunk File Format Upgrade Required", MessageBoxButton.YesNo, MessageBoxImage.Warning);
					if (result == MessageBoxResult.Yes)
					{
						EditorInstance.Chunks.StageStamps?.Write(ManiacEditor.Methods.Editor.SolutionPaths.Stamps_Source.ToString());
					}
				}
				else
				{
					EditorInstance.Chunks.StageStamps?.Write(ManiacEditor.Methods.Editor.SolutionPaths.Stamps_Source.ToString());
				}
			}
			catch (Exception ex)
			{
				Methods.Internal.Common.ShowError($@"Failed to save StageStamps to file '{ManiacEditor.Methods.Editor.SolutionPaths.Stamps_Source.SourcePath}' Error: {ex.Message}");
			}
		}

        private void AutoGenerateChunks_Click(object sender, RoutedEventArgs e)
        {
            if (Methods.Editor.Solution.EditLayerA != null && Methods.Editor.Solution.EditLayerB != null)
            {
                EditorInstance.Chunks.AutoGenerateChunks(Methods.Editor.Solution.EditLayerA, Methods.Editor.Solution.EditLayerB);
                ChunksReload();
            }
        }

        private void TrackBar2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ChunkList != null && trackBar2 != null)
            {
                if (disposing) return;
                int scale = 32 << (int)trackBar2.Value;
                ChunkList.ImageSize = scale;
            }

        }

        private void AutoGenerateChunksSingle_Click(object sender, RoutedEventArgs e)
        {
            if (Methods.Editor.Solution.EditLayerA != null)
            {
                EditorInstance.Chunks.AutoGenerateChunks(Methods.Editor.Solution.EditLayerA);
                ChunksReload();
            }
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (!ManiacEditor.Methods.Editor.SolutionState.IsChunksEdit()) SetDropdownItemsState(false);
            else SetDropdownItemsState(true);
        }

        private void SetDropdownItemsState(bool enabled)
        {
            AutoGenerateChunksSingle.IsEnabled = enabled;
            AutoGenerateChunks.IsEnabled = enabled;
            SaveChunksManually.IsEnabled = enabled;
        }
    }
}
