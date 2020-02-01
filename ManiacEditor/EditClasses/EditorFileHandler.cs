﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using RSDKv5;
using GenerationsLib.Core;
using EditClasses.Solution;

namespace ManiacEditor
{
    public class EditorFileHandler
    {
        private Editor Instance;
        public EditorFileHandler(Editor instance)
        {
            Instance = instance;
        }

        private bool EditorLoad()
        {
            if (Instance.DataDirectory == null)
            {
                return false;
            }
            Instance.EntityDrawing.ReleaseResources();
            return true;
        }

        #region Editor Commands

        public void NewScene()
        {
            if (AllowSceneUnloading() != true) return;
            CurrentSolution.UnloadScene();
            ManiacEditor.Interfaces.NewSceneWindow makerDialog = new ManiacEditor.Interfaces.NewSceneWindow();
            makerDialog.Owner = Editor.GetWindow(Instance);
            if (makerDialog.ShowDialog() == true)
            {
                string directoryPath = Path.GetDirectoryName(makerDialog.SceneFolder);

                CurrentSolution.CurrentScene = new CurrentSolution.EditorScene(Instance.FormsModel.GraphicPanel, makerDialog.Scene_Width, makerDialog.Scene_Height, makerDialog.BG_Width, makerDialog.BG_Height, Instance);
                CurrentSolution.TileConfig = new Tileconfig();
                CurrentSolution.CurrentTiles.StageTiles = new StageTiles();
                CurrentSolution.StageConfig = new StageConfig();

                string ImagePath = directoryPath + "//16x16Tiles.gif";
                string TilesPath = directoryPath + "//TilesConfig.bin";
                string StagePath = directoryPath + "//StageConfig.bin";

                File.Create(ImagePath).Dispose();
                File.Create(TilesPath).Dispose();
                File.Create(StagePath).Dispose();

                //EditorScene.Write(SceneFilepath);
                CurrentSolution.TileConfig.Write(TilesPath);
                //StageConfig.Write(StagePath);
                CurrentSolution.CurrentTiles.StageTiles.Write(ImagePath);


                Instance.UpdateDataFolderLabel(null, null);

                Instance.SetupLayerButtons();


                Instance.BackgroundDX = new EditorBackground(Instance);

                CurrentSolution.Entities = new EditorEntities(CurrentSolution.CurrentScene);

                Instance.ZoomModel.SetViewSize((int)(Instance.SceneWidth * EditClasses.EditorState.Zoom), (int)(Instance.SceneHeight * EditClasses.EditorState.Zoom));

                Instance.UI.UpdateControls(true);
            }
        }
        public void OpenScene()
        {
            if (Instance.FileHandler.AllowSceneUnloading() != true) return;
            CurrentSolution.UnloadScene();

            Instance.OpenScene();
        }
        public void OpenDataDirectory()
        {
            if (Instance.FileHandler.AllowSceneUnloading() != true) return;

            string newDataDirectory = Instance.GetDataDirectory();
            if (null == newDataDirectory) return;
            if (newDataDirectory.Equals(Instance.DataDirectory)) return;

            if (Instance.IsDataDirectoryValid(newDataDirectory)) Instance.ResetDataDirectoryToAndResetScene(newDataDirectory);
            else MessageBox.Show($@"{newDataDirectory} is not a valid Data Directory.", "Invalid Data Directory!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public void Save()
        {
            if (CurrentSolution.CurrentScene == null) return;
            if (Instance.IsTilesEdit()) Instance.Deselect();

            SaveScene();
            SaveStageConfig();
            SaveChunks();
        }
        public void SaveAs()
        {
            if (CurrentSolution.CurrentScene == null) return;
            if (Instance.IsTilesEdit()) Instance.Deselect();

            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Scene File|*.bin",
                DefaultExt = "bin",
                InitialDirectory = Instance.Paths.SceneDirectory,
                RestoreDirectory = false,
                FileName = System.IO.Path.GetFileName(Instance.Paths.SceneFile_Source)
            };
            if (save.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                string saveAsFolder = Path.GetDirectoryName(save.FileName);

                SaveScene(true, save.FileName);
                SaveAsExtras(saveAsFolder);

            }
        }
        public void UnloadScene(bool SkipCheck = false)
        {
            if (AllowSceneUnloading(SkipCheck) != true) return;
            CurrentSolution.UnloadScene();
        }
        public bool AllowSceneUnloading(bool SkipCheck = false)
        {
            if (SkipCheck) return true;
            bool AllowSceneChange = false;
            if (Instance.IsSceneLoaded() == false)
            {
                AllowSceneChange = true;
                return AllowSceneChange;
            }
            else if (Instance.IsSceneLoaded() == true && Settings.MySettings.DisableSaveWarnings == false)
            {

                if ((Instance.UndoStack.Count != 0 || Instance.RedoStack.Count != 0) || EditClasses.EditorState.QuitWithoutSavingWarningRequired == true)
                {
                    var exitBox = new UnloadingSceneWarning();
                    exitBox.Owner = Window.GetWindow(Instance);
                    exitBox.ShowDialog();
                    var exitBoxResult = exitBox.WindowResult;
                    if (exitBoxResult == UnloadingSceneWarning.WindowDialogResult.Yes)
                    {
                        SaveScene();
                        AllowSceneChange = true;
                    }
                    else if (exitBoxResult == UnloadingSceneWarning.WindowDialogResult.No)
                    {
                        AllowSceneChange = true;
                    }
                    else
                    {
                        AllowSceneChange = false;
                    }
                }
                else
                {
                    AllowSceneChange = true;
                }

            }
            else
            {
                AllowSceneChange = true;
            }
            return AllowSceneChange;
        }
        public void ExportAsPNG()
        {
            
            if (CurrentSolution.CurrentScene == null) return;

            System.Windows.Forms.SaveFileDialog save = new System.Windows.Forms.SaveFileDialog
            {
                Filter = ".png File|*.png",
                DefaultExt = "png"
            };
            if (save.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(Instance.SceneWidth, Instance.SceneHeight))
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    // not all scenes have both a Low and a High foreground
                    // only attempt to render the ones we actually have
                    Instance.FGLower?.Draw(g);
                    Instance.FGLow?.Draw(g);
                    Instance.FGHigh?.Draw(g);
                    Instance.FGHigher?.Draw(g);
                    CurrentSolution.Entities?.Draw(g);

                    bitmap.Save(save.FileName);
                }

            }
        }
        public void ExportLayersAsPNG()
        {
            try
            {
                if (CurrentSolution.CurrentScene?._editorLayers == null || !CurrentSolution.CurrentScene._editorLayers.Any()) return;

                var dialog = new GenerationsLib.Core.FolderSelectDialog()
                {
                    Title = "Select folder to save each exported layer image to"
                };

                if (!dialog.ShowDialog()) return;

                int fileCount = 0;

                foreach (var editorLayer in CurrentSolution.CurrentScene.AllLayers)
                {
                    string fileName = System.IO.Path.Combine(dialog.FileName, editorLayer.Name + ".png");

                    if (!Instance.CanWriteFile(fileName))
                    {
                        Instance.ShowError($"Layer export aborted. {fileCount} images saved.");
                        return;
                    }

                    using (var bitmap = new System.Drawing.Bitmap(editorLayer.Width * EditorConstants.TILE_SIZE, editorLayer.Height * EditorConstants.TILE_SIZE))
                    using (var g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        editorLayer.Draw(g);
                        bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                        ++fileCount;
                    }
                }

                MessageBox.Show($"Layer export succeeded. {fileCount} images saved.", "Success!",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Instance.ShowError("An error occurred: " + ex.Message);
            }
        }
        public void ExportObjLayoutAsPNG()
        {
            int i = 0;
            try
            {
                if (CurrentSolution.CurrentScene?._editorLayers == null || !CurrentSolution.CurrentScene._editorLayers.Any()) return;

                var dialog = new FolderSelectDialog()
                {
                    Title = "Select folder to save each exported object layout image to"
                };

                if (!dialog.ShowDialog()) return;

                int fileCount = 0;

                string fileName = System.IO.Path.Combine(dialog.FileName, "Objects.png");

                using (var bitmap = new System.Drawing.Bitmap(1024 * EditorConstants.TILE_SIZE, 256 * EditorConstants.TILE_SIZE))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        for (i = 0; i < CurrentSolution.Entities.Entities.Count; i++)
                        {
                            //if (!Instance.CanWriteFile(fileName))
                            // {
                            //    Instance.ShowError($"Layout export aborted. {fileCount} images saved.");
                            //    return;
                            //}
                            try
                            {
                                CurrentSolution.Entities.Entities[i].ExportDraw(g,false);
                            }
                            catch
                            {

                            }
                        }
                    }
                    bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                    ++fileCount;
                }

                System.Windows.MessageBox.Show($"Layer export succeeded. {fileCount} images saved.", "Success!",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Instance.ShowError("An error occurred: " + ex.Message);
            }
        }

        #endregion

        #region Opening
        public void OpenSceneUsingSceneSelect()
        {
            ManiacEditor.Interfaces.SceneSelectWindow select;

            if (!EditorLoad())
            {
                select = new ManiacEditor.Interfaces.SceneSelectWindow(null, Instance);
                select.Owner = Instance;
                select.ShowDialog();
            }
            else
            {
                select = new ManiacEditor.Interfaces.SceneSelectWindow(CurrentSolution.GameConfig, Instance);
                select.Owner = Instance;
                select.ShowDialog();
            }

            if (select.DialogResult != true) return;
            if (PreLoad() == false) return;

            GetSceneSelectData(select.SceneSelect, select.SceneSelect.Browsed);

            if (Instance.Paths.Browsed)
            {
                LoadFromFiles();
            }
            else
            {
                LoadFromSceneSelect();
            }

        }

        public void OpenSceneSelectFromPreviousConfiguration(DataSaveStateCollection.SaveState SaveState)
        {
            ManiacEditor.Interfaces.SceneSelectWindow select;
            Instance.Paths.SetGameConfig(SaveState.DataDirectory);

            select = new ManiacEditor.Interfaces.SceneSelectWindow((EditorLoad() ? CurrentSolution.GameConfig : null), Instance);

            select.Owner = Instance;

            if (SaveState.isDataPack)
            {
                select.SceneSelect.UnloadDataPack();
                select.SceneSelect.LoadDataPackFromName(SaveState.LoadedDataPack);
            }
            else
            {
                select.SceneSelect.UnloadDataPack();
            }

            select.ShowDialog();


            if (select.DialogResult != true) return;
            if (PreLoad() == false) return;

            GetSceneSelectData(select.SceneSelect, select.SceneSelect.Browsed);

            if (Instance.Paths.Browsed)
            {
                AddTemporaryResourcePack();
                LoadFromFiles();
            }
            else
            {
                LoadFromSceneSelect();
            }
        }

        public void OpenSceneSelectWithPrefrences(string dataDirectory)
        {
            ManiacEditor.Interfaces.SceneSelectWindow select;
            Instance.Paths.SetGameConfig(dataDirectory);

            if (!EditorLoad())
            {
                select = new ManiacEditor.Interfaces.SceneSelectWindow(null, Instance);
                select.Owner = Instance;
                select.ShowDialog();
            }
            else
            {
                select = new ManiacEditor.Interfaces.SceneSelectWindow(CurrentSolution.GameConfig, Instance);
                select.Owner = Instance;
                select.ShowDialog();
            }

            if (select.DialogResult != true) return;
            if (PreLoad() == false) return;

            GetSceneSelectData(select.SceneSelect, select.SceneSelect.Browsed);

            if (Instance.Paths.Browsed)
            {
                AddTemporaryResourcePack();
                LoadFromFiles();
            }
            else
            {
                LoadFromSceneSelect();
            }

        }

        public void OpenSceneFromSaveState(string dataDirectory, string Result, int LevelID, bool isEncore, string CurrentName, string CurrentZone, string CurrentSceneID, bool browsedFile, IList<string> ResourcePacks)
        {
            if (PreLoad() == false) return;

            if (Instance.Paths.SetGameConfig(dataDirectory) == false) return;

            GetSceneSelectData(dataDirectory, Result, LevelID, isEncore, CurrentName, CurrentZone, CurrentSceneID, browsedFile, true);

            Instance.ResourcePackList = ResourcePacks;

            if (Instance.Paths.Browsed)
            {
                LoadFromFiles();
            }
            else
            {
                LoadFromSceneSelect();
            }

        }

        public void OpenSceneUsingExistingSceneSelect(ManiacEditor.Interfaces.SceneSelect select)
        {
            if (PreLoad() == false) return;

            GetSceneSelectData(select, select.Browsed);

            if (select.Browsed)
            {
                LoadFromFiles();
            }
            else
            {
                LoadFromSceneSelect();
            }
        }

        public void GetSceneSelectData(ManiacEditor.Interfaces.SceneSelect select, bool browsedFile = false, bool skipResourcePacks = false)
        {
            if (browsedFile == true)
            {
                Instance.Paths.SceneFilePath = select.SelectedSceneResult;
                Instance.Paths.CurrentLevelID = select.LevelID;
                Instance.Paths.isEncoreMode = select.isEncore;
                Instance.Paths.SceneDirectory = Path.GetDirectoryName(Instance.Paths.SceneFilePath);
                Instance.Paths.CurrentZone = Path.GetFileName(Instance.Paths.SceneDirectory);
                Instance.Paths.CurrentName = select.CurrentName;
                Instance.Paths.CurrentSceneID = select.CurrentSceneID;
                Instance.Paths.Browsed = select.Browsed;
                if (!skipResourcePacks) AddTemporaryResourcePack();
            }
            else
            {
                Instance.Paths.SceneFilePath = select.SelectedSceneResult;
                Instance.Paths.CurrentLevelID = select.LevelID;
                Instance.Paths.isEncoreMode = select.isEncore;
                Instance.Paths.SceneDirectory = Path.GetDirectoryName(Instance.Paths.SceneFilePath);
                Instance.Paths.CurrentZone = select.CurrentZone;
                Instance.Paths.CurrentName = select.CurrentName;
                Instance.Paths.CurrentSceneID = select.CurrentSceneID;
                Instance.Paths.Browsed = select.Browsed;
            }
        }

        public void GetSceneSelectData(string dataDirectory, string Result, int LevelID, bool isEncore, string CurrentName, string CurrentZone, string CurrentSceneID, bool browsedFile, bool skipResourcePacks = false)
        {
            if (browsedFile == true)
            {
                Instance.Paths.SceneFilePath = Result;
                Instance.Paths.CurrentLevelID = LevelID;
                Instance.Paths.isEncoreMode = isEncore;
                Instance.Paths.SceneDirectory = Path.GetDirectoryName(Instance.Paths.SceneFilePath);
                Instance.Paths.CurrentZone = Path.GetFileName(Instance.Paths.SceneDirectory);
                Instance.Paths.CurrentName = CurrentName;
                Instance.Paths.CurrentSceneID = CurrentSceneID;
                Instance.Paths.Browsed = browsedFile;
                if (!skipResourcePacks) AddTemporaryResourcePack();
            }
            else
            {
                Instance.Paths.SceneFilePath = Result;
                Instance.Paths.CurrentLevelID = LevelID;
                Instance.Paths.isEncoreMode = isEncore;
                Instance.Paths.SceneDirectory = Path.GetDirectoryName(Instance.Paths.SceneFilePath);
                Instance.Paths.CurrentZone = CurrentZone;
                Instance.Paths.CurrentName = CurrentName;
                Instance.Paths.CurrentSceneID = CurrentSceneID;
                Instance.Paths.Browsed = browsedFile;
            }
        }

        public void AddTemporaryResourcePack()
        {
            if (Instance.Paths.SceneDirectory.Contains("Data") && Instance.Paths.SceneDirectory.Contains("Stages"))
            {
                var input = Instance.Paths.SceneDirectory;
                var output = input.Replace("\\Stages\\" + Instance.Paths.CurrentZone, "");
                if (output.Contains("Data"))
                {
                    Instance.ResourcePackList.Add(output);
                }
            }
        }
        #endregion

        #region Loading
        public void LoadFromSceneSelect()
        {
            bool LoadFailed = false;
            try
            {
                //Using Instance Means the Stuff Hasn't Stated 
                EditClasses.EditorState.LevelID = Instance.Paths.CurrentLevelID;
                CurrentSolution.CurrentScene = new CurrentSolution.EditorScene(Instance.Paths.GetScenePath(), Instance.FormsModel.GraphicPanel, Instance);

                //ACT File (Encore Colors)
                Instance.EncorePalette = CurrentSolution.CurrentScene.GetEncorePalette(Instance.Paths.CurrentZone, Instance.DataDirectory, Instance.Paths.CurrentSceneID, "", 1);
                EditClasses.EditorState.EncoreSetupType = CurrentSolution.CurrentScene.GetEncoreSetupType(Instance.Paths.CurrentZone, Instance.DataDirectory, Instance.Paths.CurrentSceneID, "");
                if (Instance.EncorePalette[0] != "")
                {
                    EditClasses.EditorState.EncorePaletteExists = true;
                    if (Instance.Paths.isEncoreMode)
                    {
                        Instance.EditorToolbar.EncorePaletteButton.IsChecked = true;
                        EditClasses.EditorState.UseEncoreColors = true;
                    }
                }

                //Stage Tiles
                bool valid;
                if (EditClasses.EditorState.UseEncoreColors == true && Instance.EncorePalette[0] != "") valid = Instance.Paths.GetStageTiles(Instance.Paths.CurrentZone, Instance.EncorePalette[0]);
                else valid = Instance.Paths.GetStageTiles(Instance.Paths.CurrentZone);
                if (valid == false)
                {
                    LoadingFailed("Stage Config was unable to be found or was invalid!");
                    return;
                }

                //Tile Config
                Instance.Paths.GetTileConfig(Instance.Paths.CurrentZone);
                Instance.Paths.GetStageConfig(Instance.Paths.CurrentZone);
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                LoadingFailed(ex);
                return;
            }

            if (!LoadFailed) AfterLoad();

        }

        public void AfterLoad()
        {
            try
            {
                SetupObjectsList();
                SetupDiscordRP(Instance.Paths.SceneFilePath);
                Stamps StageStamps = Instance.Paths.GetEditorStamps(Instance.Paths.CurrentZone);
                Instance.Chunks = new EditorChunk(Instance, CurrentSolution.CurrentTiles.StageTiles, StageStamps);
                Instance.BackgroundDX = new EditorBackground(Instance);
                CurrentSolution.Entities = new EditorEntities(CurrentSolution.CurrentScene);

                Instance.UI.UpdateSplineSpawnObjectsList(CurrentSolution.CurrentScene.Objects);

                ReadManiacINIFile();
                Instance.UpdateStartScreen(false);
                Instance.UpdateDataFolderLabel(null, null);
                Instance.SetupLayerButtons();
                Instance.ZoomModel.SetViewSize((int)(Instance.SceneWidth * EditClasses.EditorState.Zoom), (int)(Instance.SceneHeight * EditClasses.EditorState.Zoom));
                EditClasses.EditorState.UpdateMultiLayerSelectMode();
                Instance.UI.UpdateControls(true);
                Instance.RecentsList.AddRecentFile(Instance.RecentsList.GenerateNewEntry());
                Instance.RecentDataSourcesList.AddRecentFile(Instance.RecentDataSourcesList.GenerateNewEntry());

            }
            catch (Exception ex)
            {
                LoadingFailed(ex);
                return;
            }

        }

        public bool PreLoad()
        {
            CurrentSolution.UnloadScene();
            Instance.Settings.UseDefaultPrefrences();
            CurrentSolution.CurrentTiles = new CurrentSolution.EditorTiles(Instance);
            return Instance.SetGameConfig();
        }

        public void SetupObjectsList()
        {
            Instance.ObjectList.Clear();
            for (int i = 0; i < CurrentSolution.GameConfig.ObjectsNames.Count; i++)
            {
                Instance.ObjectList.Add(CurrentSolution.GameConfig.ObjectsNames[i]);
            }
            for (int i = 0; i < CurrentSolution.StageConfig.ObjectsNames.Count; i++)
            {
                Instance.ObjectList.Add(CurrentSolution.StageConfig.ObjectsNames[i]);
            }
        }

        public void SetupDiscordRP(string SceneFile)
        {
            DiscordRP.UpdateDiscord(SceneFile);
        }

        public void LoadFromFiles()
        {
            bool LoadFailed = false;
            try
            {
                EditClasses.EditorState.LevelID = Instance.Paths.CurrentLevelID;
                CurrentSolution.CurrentScene = new CurrentSolution.EditorScene(Instance.Paths.GetScenePathFromFile(Instance.Paths.SceneFilePath), Instance.FormsModel.GraphicPanel, Instance);


                //ACT File (Encore Colors)
                Instance.EncorePalette = CurrentSolution.CurrentScene.GetEncorePalette(Instance.Paths.CurrentZone, Instance.DataDirectory, Instance.Paths.CurrentSceneID, Instance.Paths.SceneDirectory, 0);
                EditClasses.EditorState.EncoreSetupType = CurrentSolution.CurrentScene.GetEncoreSetupType(Instance.Paths.CurrentZone, Instance.DataDirectory, Instance.Paths.CurrentSceneID, Instance.Paths.SceneDirectory);
                if (Instance.EncorePalette[0] != "")
                {
                    EditClasses.EditorState.EncorePaletteExists = true;
                    if (Instance.Paths.isEncoreMode)
                    {
                        Instance.EditorToolbar.EncorePaletteButton.IsChecked = true;
                        EditClasses.EditorState.UseEncoreColors = true;
                    }
                }

                //Stage Tiles
                bool valid;
                if (EditClasses.EditorState.UseEncoreColors == true && Instance.EncorePalette[0] != "") valid = Instance.Paths.GetStageTiles(Instance.Paths.CurrentZone, Instance.EncorePalette[0], Instance.Paths.Browsed);
                else valid = Instance.Paths.GetStageTiles(Instance.Paths.CurrentZone, null, Instance.Paths.Browsed);
                if (valid == false)
                {
                    LoadingFailed("Stage Config was ethier Invalid or Not Found!");
                    return;
                }

                //Tile Config
                Instance.Paths.GetTileConfig(Instance.Paths.CurrentZone, Instance.Paths.Browsed);
                Instance.Paths.GetStageConfig(Instance.Paths.CurrentZone, Instance.Paths.Browsed);
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                LoadingFailed(ex);
                return;
            }


            if (!LoadFailed) AfterLoad();
        }

        public void ReadManiacINIFile()
        {
            Instance.ManiacINI.ClearSettings();
            if (File.Exists(Instance.Paths.SceneFile_Directory + "\\maniac.json"))
            {
                Instance.ManiacINI.UpdateFilePath();
                Instance.ManiacINI.LoadFile();

            }
        }


        public void LoadingFailed(Exception ex)
        {
            MessageBox.Show("Load failed. Error: " + ex.ToString());
            UnloadScene(true);
        }

        public void LoadingFailed(string ex)
        {
            MessageBox.Show("Load failed. Error: " + ex);
            UnloadScene(true);
        }
        #endregion

        #region Saving

        public void SaveAsExtras(string saveAsFolder)
        {
            string stageConfig = Path.Combine(saveAsFolder, "StageConfig.bin");
            string stamps = Path.Combine(saveAsFolder, "ManiacStamps.bin");
            string stageTiles = Path.Combine(saveAsFolder, "16x16Tiles.gif");
            string tilesConfig = Path.Combine(saveAsFolder, "TileConfig.bin");

            bool stageConfigExists = File.Exists(stageConfig);
            bool stampsExists = File.Exists(stamps);
            bool stageTilesExists = File.Exists(stageTiles);
            bool tilesConfigExists = File.Exists(tilesConfig);

            if (!stageConfigExists || !stampsExists || !stageTilesExists || !tilesConfigExists)
            {
                string newLine = Environment.NewLine;
                string missingFiles = "The following files do not exist in this location:";
                if (!stageConfigExists) missingFiles += newLine + "StageConfig.bin";
                if (!stampsExists) missingFiles += newLine + "ManiacStamps.bin";
                if (!stageTilesExists) missingFiles += newLine + "16x16Tiles.gif";
                if (!tilesConfigExists) missingFiles += newLine + "TileConfig.bin";
                missingFiles += newLine + newLine + "Would you like to make these files? (If you don't this may screw up the loading system)";
                MessageBoxResult result = MessageBox.Show(missingFiles, "Missing Files", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (!stageConfigExists) SaveStageConfig(true, stageConfig);
                    if (!stampsExists) SaveChunks(true, stamps);
                    if (!stageTilesExists) Save16x16Tiles(true, stageTiles);
                    if (!tilesConfigExists) SaveTilesConfig(true, tilesConfig);
                }
            }


        }

		public void SaveScene(bool saveAsMode = false, string SaveAsFilePath = "")
		{
			try
			{
                if (saveAsMode)
                {
                    CurrentSolution.CurrentScene.Save(SaveAsFilePath);
                    Instance.Paths.SceneFile_Source = SaveAsFilePath;
                    Instance.Paths.SceneFile_SourceID = -3;
                }
                else
                {
                    if (EditClasses.EditorState.DataDirectoryReadOnlyMode && Instance.Paths.SceneFile_SourceID == -1) return;
                    else CurrentSolution.CurrentScene.Save(Instance.Paths.SceneFile_Source);
                }

			}
			catch (Exception ex)
			{
				Instance.ShowError($@"Failed to save the scene to file '{Instance.Paths.SceneFile_Source}' Error: {ex.Message}");
			}
		}

		public void SaveStageConfig(bool saveAsMode = false, string SaveAsFilePath = "")
		{
			try
			{
                if (saveAsMode)
                {
                    CurrentSolution.StageConfig?.Write(SaveAsFilePath);
                    Instance.Paths.StageConfig_Source = SaveAsFilePath;
                    Instance.Paths.StageConfig_SourceID = -3;
                }
                else
                {
                    if (EditClasses.EditorState.DataDirectoryReadOnlyMode && Instance.Paths.StageConfig_SourceID == -1) return;
                    else CurrentSolution.StageConfig?.Write(Instance.Paths.StageConfig_Source);
                }

			}
			catch (Exception ex)
			{
				Instance.ShowError($@"Failed to save the StageConfig to file '{Instance.Paths.StageConfig_Source}' Error: {ex.Message}");
			}
		}

        public void SaveChunks(bool saveAsMode = false, string SaveAsFilePath = "")
        {
            try
            {
                if (Instance.Chunks.StageStamps?.loadstate == RSDKv5.Stamps.LoadState.Upgrade)
                {
                    MessageBoxResult result = MessageBox.Show("This Editor Chunk File needs to be updated to a newer version of the format. This will happen almost instantly, however you will be unable to use your chunks in a previous version of maniac on this is done. Would you like to continue?" + Environment.NewLine + "(Click Yes to Save, Click No to Continue without Saving Your Chunks)", "Chunk File Format Upgrade Required", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes) return;
                }
                if (saveAsMode)
                {
                    Instance.Chunks.StageStamps?.Write(SaveAsFilePath);
                    Instance.Paths.Stamps_Source = SaveAsFilePath;
                    Instance.Paths.Stamps_SourceID = -3;
                }
                else
                {
                    if (EditClasses.EditorState.DataDirectoryReadOnlyMode && Instance.Paths.Stamps_SourceID == -1) return;
                    else Instance.Chunks.StageStamps?.Write(Instance.Paths.Stamps_Source);
                }



            }
            catch (Exception ex)
            {
                Instance.ShowError($@"Failed to save StageStamps to file '{Instance.Paths.SceneFile_Source}' Error: {ex.Message}");
            }
        }

        public void SaveTilesConfig(bool saveAsMode = false, string SaveAsFilePath = "")
        {
            try
            {
                if (saveAsMode)
                {
                    CurrentSolution.TileConfig?.Write(SaveAsFilePath);
                    Instance.Paths.TileConfig_Source = SaveAsFilePath;
                    Instance.Paths.TileConfig_SourceID = -3;
                }
            }
            catch (Exception ex)
            {
                Instance.ShowError($@"Failed to save the TileConfig to file '{Instance.Paths.StageConfig_Source}' Error: {ex.Message}");
            }
        }

        public void Save16x16Tiles(bool saveAsMode = false, string SaveAsFilePath = "")
        {
            try
            {
                if (saveAsMode)
                {
                    CurrentSolution.CurrentTiles.StageTiles?.Write(SaveAsFilePath);
                    Instance.Paths.StageTiles_Source = SaveAsFilePath;
                    Instance.Paths.StageTiles_SourceID = -3;
                }
            }
            catch (Exception ex)
            {
                Instance.ShowError($@"Failed to save the 16x16Tiles.gif to file '{Instance.Paths.StageConfig_Source}' Error: {ex.Message}");
            }
        }
        #endregion
    }
}
