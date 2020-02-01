﻿using System;
using System.Linq;
using System.Windows;
using System.IO;
using System.Windows.Forms;
using RSDKv5;
using Cyotek.Windows.Forms;
using Color = System.Drawing.Color;
using System.Runtime.InteropServices;
using System.Diagnostics;
using EditClasses.Solution;

namespace ManiacEditor
{
    public static class EditorLaunch
    {
        #region Variables/DLL Imports
        private static Editor Editor;
        public static ManiacED_ManiaPal.Connector ManiaPalConnector;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };
        #endregion

        public static void UpdateInstance(Editor instance)
        {
            Editor = instance;
        }

        #region Apps

        public static void CheatEngine()
        {
            String cheatEngineProcessName = Path.GetFileNameWithoutExtension(ManiacEditor.Settings.MyDefaults.CheatEnginePath);
            IntPtr hWnd = FindWindow(cheatEngineProcessName, null); // this gives you the handle of the window you need.
            Process processes = Process.GetProcessesByName(cheatEngineProcessName).FirstOrDefault();
            if (processes != null)
            {
                // check if the window is hidden / minimized
                if (processes.MainWindowHandle == IntPtr.Zero)
                {
                    // the window is hidden so try to restore it before setting focus.
                    ShowWindow(processes.Handle, ShowWindowEnum.Restore);
                }

                // set user the focus to the window
                SetForegroundWindow(processes.MainWindowHandle);
            }
            else
            {
                // Ask where the Mania Mod Manager is located when not set
                if (string.IsNullOrEmpty(ManiacEditor.Settings.MyDefaults.CheatEnginePath))
                {
                    var ofd = new OpenFileDialog
                    {
                        Title = "Select Cheat Engine Executable",
                        Filter = "Windows PE Executable|*.exe"
                    };
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        ManiacEditor.Settings.MyDefaults.CheatEnginePath = ofd.FileName;
                }
                else
                {
                    if (!File.Exists(ManiacEditor.Settings.MyDefaults.CheatEnginePath))
                    {
                        ManiacEditor.Settings.MyDefaults.CheatEnginePath = "";
                        return;
                    }
                }

                if (File.Exists(ManiacEditor.Settings.MyDefaults.CheatEnginePath))
                    Process.Start(ManiacEditor.Settings.MyDefaults.CheatEnginePath);
            }
        }
        public static void TileManiacNormal()
        {
            if (Editor.TileManiacInstance == null || Editor.TileManiacInstance.IsClosed) Editor.TileManiacInstance = new ManiacEditor.MainWindow();
            Editor.TileManiacInstance.Show();
            if (CurrentSolution.TileConfig != null && CurrentSolution.CurrentTiles.StageTiles != null)
            {
                if (Editor.TileManiacInstance.Visibility != Visibility.Visible || Editor.TileManiacInstance.tcf == null)
                {
                    Editor.TileManiacInstance.LoadTileConfigViaIntergration(CurrentSolution.TileConfig, Editor.Paths.TileConfig_Source);
                }
                else
                {
                    Editor.TileManiacInstance.Activate();
                }

            }

        }
        public static void TileManiacIntergration()
        {
            try
            {
                if (Editor.TileManiacInstance == null || Editor.TileManiacInstance.IsClosed) Editor.TileManiacInstance = new ManiacEditor.MainWindow();
                if (Editor.TileManiacInstance.Visibility != Visibility.Visible)
                {
                    Editor.TileManiacInstance.Show();
                }
                if (CurrentSolution.TileConfig != null && CurrentSolution.CurrentTiles.StageTiles != null)
                {
                    if (Editor.TileManiacInstance.Visibility != Visibility.Visible || Editor.TileManiacInstance.tcf == null)
                    {
                        Editor.TileManiacInstance.LoadTileConfigViaIntergration(CurrentSolution.TileConfig, Editor.Paths.TileConfig_Source, EditClasses.EditorState.SelectedTileID);
                    }
                    else
                    {
                        Editor.TileManiacInstance.SetCollisionIndex(EditClasses.EditorState.SelectedTileID);
                        Editor.TileManiacInstance.Activate();
                    }

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return;
            }

        }
        public static void ManiaModManager()
        {
            String modProcessName = Path.GetFileNameWithoutExtension(ManiacEditor.Settings.MyDefaults.ModLoaderPath);
            IntPtr hWnd = FindWindow(modProcessName, null); // this gives you the handle of the window you need.
            Process processes = Process.GetProcessesByName(modProcessName).FirstOrDefault();
            if (processes != null)
            {
                // check if the window is hidden / minimized
                if (processes.MainWindowHandle == IntPtr.Zero)
                {
                    // the window is hidden so try to restore it before setting focus.
                    ShowWindow(processes.Handle, ShowWindowEnum.Restore);
                }

                // set user the focus to the window
                SetForegroundWindow(processes.MainWindowHandle);
            }
            else
            {
                // Ask where the Mania Mod Manager is located when not set
                if (string.IsNullOrEmpty(ManiacEditor.Settings.MyDefaults.ModLoaderPath))
                {
                    var ofd = new OpenFileDialog
                    {
                        Title = "Select Mania Mod Manager.exe",
                        Filter = "Windows PE Executable|*.exe"
                    };
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        ManiacEditor.Settings.MyDefaults.ModLoaderPath = ofd.FileName;
                }
                else
                {
                    if (!File.Exists(ManiacEditor.Settings.MyDefaults.ModLoaderPath))
                    {
                        ManiacEditor.Settings.MyDefaults.ModLoaderPath = "";
                        return;
                    }
                }

                if (File.Exists(ManiacEditor.Settings.MyDefaults.ModLoaderPath))
                    Process.Start(ManiacEditor.Settings.MyDefaults.ModLoaderPath);
            }
        }
        public static void RSDKUnpacker()
        {

        }
        public static void InsanicManiac()
        {
            //Sanic2Maniac sanic = new Sanic2Maniac(null, this);
            //sanic.Show();
        }
        public static void RSDKAnnimationEditor()
        {
            String aniProcessName = Path.GetFileNameWithoutExtension(Settings.MyDefaults.AnimationEditorPath);
            IntPtr hWnd = FindWindow(aniProcessName, null); // this gives you the handle of the window you need.
            Process processes = Process.GetProcessesByName(aniProcessName).FirstOrDefault();
            if (processes != null)
            {
                // check if the window is hidden / minimized
                if (processes.MainWindowHandle == IntPtr.Zero)
                {
                    // the window is hidden so try to restore it before setting focus.
                    ShowWindow(processes.Handle, ShowWindowEnum.Restore);
                }

                // set user the focus to the window
                SetForegroundWindow(processes.MainWindowHandle);
            }
            else
            {

                // Ask where RSDK Annimation Editor is located when not set
                if (string.IsNullOrEmpty(Settings.MyDefaults.AnimationEditorPath))
                {
                    var ofd = new OpenFileDialog
                    {
                        Title = "Select RSDK Animation Editor.exe",
                        Filter = "Windows Executable|*.exe"
                    };
                    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        Settings.MyDefaults.AnimationEditorPath = ofd.FileName;
                }
                else
                {
                    if (!File.Exists(Settings.MyDefaults.AnimationEditorPath))
                    {
                        Settings.MyDefaults.AnimationEditorPath = "";
                        return;
                    }
                }

                ProcessStartInfo psi;
                psi = new ProcessStartInfo(Settings.MyDefaults.AnimationEditorPath);
                Process.Start(psi);
            }
        }
        public static void RenderListManager()
        {
            RenderListEditor editor = new RenderListEditor(Editor);
            editor.Owner = Editor.Owner;
            editor.ShowDialog();
        }
        public static void ManiaPal(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem button = sender as System.Windows.Controls.MenuItem;
            bool isGameConfig = false;
            bool GC_NULL = false;
            bool SC_NULL = false;
            string SC_Path = "";
            string GC_Path = "";

            if (button != null && button == Editor.EditorMenuBar.maniaPalGameConfigToolStripMenuItem)
            {
                if (CurrentSolution.GameConfig == null) GC_NULL = true;
                else GC_Path = CurrentSolution.GameConfig.FilePath;
                isGameConfig = true;
            }
            else
            {
                if (CurrentSolution.StageConfig == null) SC_NULL = true;
                else SC_Path = CurrentSolution.StageConfig.FilePath;
                isGameConfig = false;
            }


            if (ManiaPalConnector == null) ManiaPalConnector = new ManiacED_ManiaPal.Connector();

            ManiaPalConnector.SetLoadingInformation(GC_Path, SC_Path, SC_NULL, GC_NULL);
            ManiaPalConnector.Activate(isGameConfig);



        }
        public static void ManiaPalSubmenuOpened(object sender, RoutedEventArgs e)
        {
            Editor.EditorMenuBar.maniaPalHint.Header = "HINT: The Button that houses this dropdown" + Environment.NewLine + "will focus ManiaPal if it is opened already" + Environment.NewLine + "(without reloading the currently loaded colors)";
        }
        public static void SonicManiaHeadless()
        {
            Editor.InGame.RunSequence(null, null, false);
        }
        public static void DuplicateObjectIDHealer()
        {
            MessageBoxResult result = System.Windows.MessageBox.Show("WARNING: Once you do this the editor will restart immediately, make sure your progress is closed and saved!", "WARNING", MessageBoxButton.OKCancel, MessageBoxImage.Information);
            if (result == MessageBoxResult.OK)
            {
                string Result = null;
                OpenFileDialog open = new OpenFileDialog() { };
                open.Filter = "Scene File|*.bin";
                if (open.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
                {
                    Result = open.FileName;
                }

                if (Result == null)
                    return;

                CurrentSolution.UnloadScene();
                Editor.Settings.UseDefaultPrefrences();

                ObjectIDHealer healer = new ObjectIDHealer();
                Editor.ShowConsoleWindow();
                healer.startHealing(open.FileName);
                Editor.HideConsoleWindow();
            }
        }

        #endregion

        #region Folders

        public static void OpenFolder(string folderPath)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        public static void OpenSceneFolder()
        {
            if (Editor.Paths.SceneFile_Directory != null && Editor.Paths.SceneFile_Directory != "")
            {
                string SceneFilename_mod = Editor.Paths.SceneFile_Directory.Replace('/', '\\');
                OpenFolder(SceneFilename_mod);
            }
            else
            {
                System.Windows.MessageBox.Show("Scene File does not exist or simply isn't loaded!", "ERROR");
            }

        }
        public static void OpenManiacEditorFolder()
        {
            OpenFolder(Environment.CurrentDirectory);
        }
        public static void OpenManiacEditorFixedSettingsFolder()
        {
            OpenFolder(EditorConstants.SettingsStaticDirectory);
        }
        public static void OpenManiacEditorPortableSettingsFolder()
        {
            OpenFolder(EditorConstants.SettingsPortableDirectory);
        }
        public static void OpenDataDirectory()
        {
            string DataDirectory_mod = Editor.DataDirectory.Replace('/', '\\');
            if (DataDirectory_mod != null && DataDirectory_mod != "" && Directory.Exists(DataDirectory_mod))
            {
                OpenFolder(DataDirectory_mod);
            }
            else
            {
                System.Windows.MessageBox.Show("Data Directory does not exist or simply isn't loaded!", "ERROR");
            }

        }
        public static void OpenSonicManiaFolder()
        {
            if (Settings.MyDefaults.SonicManiaPath != null && Settings.MyDefaults.SonicManiaPath != "" && File.Exists(Settings.MyDefaults.SonicManiaPath))
            {
                string GameFolder = Settings.MyDefaults.SonicManiaPath;
                string GameFolder_mod = GameFolder.Replace('/', '\\');
                Process.Start("explorer.exe", "/select, " + GameFolder_mod);
            }
            else
            {
                System.Windows.MessageBox.Show("Game Folder does not exist or isn't set!", "ERROR");
            }

        }

        #region Saved Place
        public static void OpenASavedPlaceDropDownOpening(object sender, RoutedEventArgs e)
        {
            if (Settings.MySettings.SavedPlaces != null && Settings.MySettings.SavedPlaces.Count > 0)
            {
                Editor.EditorMenuBar.openASavedPlaceToolStripMenuItem.Items.Clear();
                var allItems = Editor.EditorMenuBar.openASavedPlaceToolStripMenuItem.Items.Cast<System.Windows.Controls.MenuItem>().ToArray();
                foreach (string savedPlace in Settings.MySettings.SavedPlaces)
                {
                    var savedPlaceItem = new System.Windows.Controls.MenuItem()
                    {
                        Header = savedPlace,
                        Tag = savedPlace
                    };
                    savedPlaceItem.Click += OpenASavedPlaceTrigger;
                    Editor.EditorMenuBar.openASavedPlaceToolStripMenuItem.Items.Add(savedPlaceItem);
                }
            }

        }

        public static void OpenASavedPlaceTrigger(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem item = sender as System.Windows.Controls.MenuItem;
            string savedPlaceDir = item.Header.ToString().Replace('/', '\\');
            if (Directory.Exists(savedPlaceDir))
            {
                OpenFolder(savedPlaceDir);
            }
            else
            {
                System.Windows.MessageBox.Show("This Folder does not exist! " + string.Format("({0})", savedPlaceDir), "ERROR");
            }
        }

        public static void OpenASavedPlaceDropDownClosed(object sender, RoutedEventArgs e)
        {
            Editor.EditorMenuBar.openASavedPlaceToolStripMenuItem.Items.Clear();
            Editor.EditorMenuBar.openASavedPlaceToolStripMenuItem.Items.Add("No Saved Places");
        }
        #endregion

        #region Data Packs
        public static void OpenAResourcePackFolderDropDownOpening(object sender, RoutedEventArgs e)
        {
            if (CurrentSolution.CurrentScene == null) Editor.ResourcePackList.Clear();
            if (Editor.ResourcePackList != null && Editor.ResourcePackList.Count > 0)
            {
                Editor.EditorMenuBar.openAResourcePackFolderToolStripMenuItem.Items.Clear();
                var allItems = Editor.EditorMenuBar.openAResourcePackFolderToolStripMenuItem.Items.Cast<System.Windows.Controls.MenuItem>().ToArray();
                foreach (string savedPlace in Editor.ResourcePackList)
                {
                    var savedPlaceItem = new System.Windows.Controls.MenuItem()
                    {
                        Header = savedPlace,
                        Tag = savedPlace
                    };
                    savedPlaceItem.Click += OpenAResourcePackFolderTrigger;
                    Editor.EditorMenuBar.openAResourcePackFolderToolStripMenuItem.Items.Add(savedPlaceItem);
                }
            }

        }

        public static void OpenAResourcePackFolderTrigger(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem item = sender as System.Windows.Controls.MenuItem;
            string resourcePackDir = item.Header.ToString().Replace('/', '\\');
            if (Directory.Exists(resourcePackDir))
            {
                OpenFolder(resourcePackDir);
            }
            else
            {
                System.Windows.MessageBox.Show("This Folder does not exist! " + string.Format("({0})", resourcePackDir), "ERROR");
            }
        }

        public static void OpenAResourcePackFolderDropDownClosed(object sender, RoutedEventArgs e)
        {
            Editor.EditorMenuBar.openAResourcePackFolderToolStripMenuItem.Items.Clear();
            Editor.EditorMenuBar.openAResourcePackFolderToolStripMenuItem.Items.Add("No Resource Packs Loaded");
        }
        #endregion

        #endregion

        #region Scene Tools

        #region Scene Tab Buttons
        public static void ImportObjectsToolStripMenuItem_Click(Window window = null)
        {
            EditClasses.EditorState.isImportingObjects = true;
            try
            {
                Scene sourceScene = Editor.GetSceneSelection();
                if (sourceScene == null) return;
                var objectImporter = new ManiacEditor.Interfaces.ObjectImporter(sourceScene.Objects, CurrentSolution.CurrentScene.Objects, CurrentSolution.StageConfig, Editor);
                if (window != null) objectImporter.Owner = window;
                objectImporter.ShowDialog();

                if (objectImporter.DialogResult != true)
                    return; // nothing to do

                // user clicked Import, get to it!
                Editor.UI.UpdateControls();
                Editor.EntitiesToolbar?.RefreshSpawningObjects(CurrentSolution.CurrentScene.Objects);
                Editor.UI.UpdateSplineSpawnObjectsList(CurrentSolution.CurrentScene.Objects);

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to import Objects. " + ex.Message);
            }
            EditClasses.EditorState.isImportingObjects = false;
        }

        public static void ImportObjectsWithMegaList(Window window = null)
        {
            EditClasses.EditorState.isImportingObjects = true;
            try
            {
                GenerationsLib.Core.FolderSelectDialog ofd = new GenerationsLib.Core.FolderSelectDialog();
                ofd.Title = "Select a clean Data Folder";
                if (ofd.ShowDialog() == true)
                {
                    string gameConfigPath = System.IO.Path.Combine(ofd.FileName, "Game", "GameConfig.bin");
                    if (File.Exists(gameConfigPath))
                    {
                        Gameconfig SourceConfig = new Gameconfig(gameConfigPath);
                        var objectImporter = new ManiacEditor.Interfaces.ObjectImporter(ofd.FileName, SourceConfig, CurrentSolution.CurrentScene.Objects, CurrentSolution.StageConfig, Editor);
                        if (window != null) objectImporter.Owner = window;
                        objectImporter.ShowDialog();

                        if (objectImporter.DialogResult != true)
                            return; // nothing to do

                        // user clicked Import, get to it!
                        Editor.UI.UpdateControls();
                        Editor.EntitiesToolbar?.RefreshSpawningObjects(CurrentSolution.CurrentScene.Objects);
                        Editor.UI.UpdateSplineSpawnObjectsList(CurrentSolution.CurrentScene.Objects);
                    }
                }


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to import Objects. " + ex.Message);
            }
            EditClasses.EditorState.isImportingObjects = false;
        }

        public static void ImportSoundsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImportSounds(Window.GetWindow(Editor));
        }
        public static void ImportSounds(Window window = null)
        {
            try
            {
                StageConfig sourceStageConfig = null;
                using (var fd = new OpenFileDialog())
                {
                    fd.Filter = "Stage Config File|*.bin";
                    fd.DefaultExt = ".bin";
                    fd.Title = "Select Stage Config File";
                    fd.InitialDirectory = Path.Combine(Editor.DataDirectory, "Stages");
                    if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            sourceStageConfig = new StageConfig(fd.FileName);
                        }
                        catch
                        {
                            System.Windows.MessageBox.Show("Ethier this isn't a stage config, or this stage config is ethier corrupted or unreadable in Maniac.");
                            return;
                        }

                    }
                }
                if (null == sourceStageConfig) return;

                var soundImporter = new ManiacEditor.Interfaces.SoundImporter(sourceStageConfig, CurrentSolution.StageConfig);
                soundImporter.ShowDialog();

                if (soundImporter.DialogResult != true)
                    return; // nothing to do


                // changing the sound list doesn't require us to do anything either

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to import sounds. " + ex.Message);
            }
            EditClasses.EditorState.QuitWithoutSavingWarningRequired = true;
        }

        public static void ManiacINIEditor(object sender, RoutedEventArgs e)
        {
            ManiacINIEditor editor = new ManiacINIEditor(Editor);
            if (editor.Owner != null) editor.Owner = Window.GetWindow(Editor);
            else editor.Owner = System.Windows.Application.Current.MainWindow;
            editor.ShowDialog();
        }

        public static void LayerManager(object sender, RoutedEventArgs e)
        {
            Editor.Deselect(true);

            var lm = new ManiacEditor.Interfaces.LayerManager(CurrentSolution.CurrentScene);
            lm.Owner = Window.GetWindow(Editor);
            lm.ShowDialog();

            
            Editor.SetupLayerButtons();
            Editor.ZoomModel.ResetViewSize();
            Editor.UI.UpdateControls();
            EditClasses.EditorState.QuitWithoutSavingWarningRequired = true;
        }

        public static void ExportGUI(object sender, RoutedEventArgs e)
        {
            var eG = new ManiacEditor.Interfaces.ExportAsImageGUI(CurrentSolution.CurrentScene);
            eG.Owner = Window.GetWindow(Editor);
            eG.ShowDialog();

        }

        public static void ObjectManager()
        {
            var objectManager = new ManiacEditor.Interfaces.ObjectManager(CurrentSolution.CurrentScene.Objects, CurrentSolution.StageConfig, Editor);
            objectManager.Owner = Window.GetWindow(Editor);
            objectManager.ShowDialog();
            EditClasses.EditorState.QuitWithoutSavingWarningRequired = true;
        }

        public static void AboutScreen()
        {
            var aboutBox = new ManiacEditor.Interfaces.AboutWindow();
            aboutBox.Owner = Editor;
            aboutBox.ShowDialog();
        }

        public static void OptionsMenu()
        {
            var optionMenu = new ManiacEditor.Interfaces.OptionsMenu();
            optionMenu.Owner = Editor;
            optionMenu.ShowDialog();
        }

        public static void ControlMenu()
        {
            var optionMenu = new ManiacEditor.Interfaces.OptionsMenu();
            optionMenu.Owner = Editor;
            optionMenu.MainTabControl.SelectedIndex = 2;
            optionMenu.ShowDialog();
        }

        public static void WikiLink()
        {
            System.Diagnostics.Process.Start("https://maniaceditor-generationsedition.readthedocs.io/en/latest/");
        }

        public static void InGameSettings()
        {
            CheatCodeManager cheatCodeManager = new CheatCodeManager();
            cheatCodeManager.Owner = Editor;
            cheatCodeManager.ShowDialog();
        }

        public static void ChangePrimaryBackgroundColor(object sender, RoutedEventArgs e)
        {
            ColorPickerDialog colorSelect = new ColorPickerDialog
            {
                Color = Color.FromArgb(CurrentSolution.CurrentScene.EditorMetadata.BackgroundColor1.R, CurrentSolution.CurrentScene.EditorMetadata.BackgroundColor1.G, CurrentSolution.CurrentScene.EditorMetadata.BackgroundColor1.B)
            };
            Editor.Theming.UseExternalDarkTheme(colorSelect);
            System.Windows.Forms.DialogResult result = colorSelect.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                {
                    RSDKv5.Color returnColor = new RSDKv5.Color
                    {
                        R = colorSelect.Color.R,
                        A = colorSelect.Color.A,
                        B = colorSelect.Color.B,
                        G = colorSelect.Color.G
                    };
                    CurrentSolution.CurrentScene.EditorMetadata.BackgroundColor1 = returnColor;
                }

            }
        }

        public static void ChangeSecondaryBackgroundColor(object sender, RoutedEventArgs e)
        {
            ColorPickerDialog colorSelect = new ColorPickerDialog
            {
                Color = Color.FromArgb(CurrentSolution.CurrentScene.EditorMetadata.BackgroundColor2.R, CurrentSolution.CurrentScene.EditorMetadata.BackgroundColor2.G, CurrentSolution.CurrentScene.EditorMetadata.BackgroundColor2.B)
            };
            Editor.Theming.UseExternalDarkTheme(colorSelect);
            System.Windows.Forms.DialogResult result = colorSelect.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                {
                    RSDKv5.Color returnColor = new RSDKv5.Color
                    {
                        R = colorSelect.Color.R,
                        A = colorSelect.Color.A,
                        B = colorSelect.Color.B,
                        G = colorSelect.Color.G
                    };
                    CurrentSolution.CurrentScene.EditorMetadata.BackgroundColor2 = returnColor;
                }

            }
        }

        #endregion

        #endregion

        #region Dev

        public static void DevTerm()
        {
            var DevController = new ManiacEditor.Interfaces.DeveloperTerminal(Editor);
            DevController.Owner = Window.GetWindow(Editor);
            DevController.Show();
        }

        #endregion
    }
}
