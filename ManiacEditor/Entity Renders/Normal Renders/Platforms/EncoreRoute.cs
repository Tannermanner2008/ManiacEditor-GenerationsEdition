﻿using System;
using System.Drawing;
using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class EncoreRoute : EntityRenderer
    {

        private SceneLayer _layer;
        internal SceneLayer Layer { get => _layer; }

        static int TILE_SIZE = 16;

        public override void Draw(Structures.EntityRenderProp properties)
        {
            Methods.Draw.GraphicsHandler d = properties.Graphics;
            SceneEntity entity = properties.Object; 
            Classes.Scene.Sets.EditorEntity e = properties.EditorObject;
            int x = properties.X;
            int y = properties.Y;
            int Transparency = properties.Transparency;
            int index = properties.Index;
            int previousChildCount = properties.PreviousChildCount;
            int platformAngle = properties.PlatformAngle;
            Methods.Entities.EntityAnimator Animation = properties.Animations;
            bool selected  = properties.isSelected;
            if (Methods.Editor.Solution.CurrentScene?.Scratch != null)
            {
                Classes.Scene.Sets.EditorLayer Scratch = Methods.Editor.Solution.CurrentScene?.Scratch;

                _layer = Scratch.Layer;
                bool fliph = false;
                bool flipv = false;
                int width = (int)entity.attributesMap["size"].ValueVector2.X.High;
                int height = (int)entity.attributesMap["size"].ValueVector2.Y.High;
                int x2 = (int)entity.attributesMap["offset"].ValueVector2.X.High;
                int y2 = (int)entity.attributesMap["offset"].ValueVector2.Y.High;

                // Prevents Out of Bounds with EncoreRoute
                if (entity.Object.Name.Name == "EncoreRoute")
                {
                    bool outOfBoundsX = false;
                    bool outOfBoundsY = false;
                    if (x2 > Methods.Editor.Solution.ScratchLayer.Width)
                    {
                        outOfBoundsX = true;
                    }
                    if (y2 > Methods.Editor.Solution.ScratchLayer.Height)
                    {
                        outOfBoundsY = true;
                    }
                    if ((y2 + height) > Scratch.Layer.Height)
                    {
                        if (outOfBoundsY)
                        {
                            System.Windows.MessageBox.Show("Layer Out of Bounds!    " + "\n" + "Y2: " + y2 + "\n" + "Height: " + height + "\n" + "Combined: " + (y2 + height) + "\n" + "Layer Height: " + Scratch.Layer.Height, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            entity.attributesMap["offset"].ValueVector2 = new Position((short)x2, 0);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Layer Out of Bounds!    " + "\n" + "Y2: " + y2 + "\n" + "Height: " + height + "\n" + "Combined: " + (y2 + height) + "\n" + "Layer Height: " + Scratch.Layer.Height, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            entity.attributesMap["size"].ValueVector2 = new Position((short)width, 0);
                        }
                    }
                    if ((x2 + width) > Scratch.Layer.Width)
                    {
                        if (outOfBoundsX)
                        {
                            System.Windows.MessageBox.Show("Layer Out of Bounds!    " + "\n" + "X2: " + x2 + "\n" + "Width: " + width + "\n" + "Combined: " + (x2 + width) + "\n" + "Layer Width: " + Scratch.Layer.Width, "", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            entity.attributesMap["offset"].ValueVector2 = new Position(0, (short)y2);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Layer Out of Bounds!    " + "\n" + "X2: " + x2 + "\n" + "Width: " + width + "\n" + "Combined: " + (x2 + width) + "\n" + "Layer Width: " + Scratch.Layer.Width);
                            entity.attributesMap["size"].ValueVector2 = new Position(0, (short)height);
                        }
                    }
                }

                var editorAnim = Controls.Editor.MainEditor.Instance.EntityDrawing.LoadAnimation2("EditorIcons2", d.DevicePanel, 0, 7, fliph, flipv, false);

                if (editorAnim != null && editorAnim.Frames.Count != 0)
                {
                    //Draw the Encore Route Tiles
                    DrawTileGroup(d, x / 16, y / 16, x2, y2, height, width, Transparency, entity, Controls.Editor.MainEditor.Instance);

                    var frame = editorAnim.Frames[Animation.index];

                    d.DrawBitmap(new Methods.Draw.GraphicsHandler.GraphicsInfo(frame),
                        x + frame.Frame.PivotX - (fliph ? (frame.Frame.Width - editorAnim.Frames[0].Frame.Width) : 0),
                        y + frame.Frame.PivotY + (flipv ? (frame.Frame.Height - editorAnim.Frames[0].Frame.Height) : 0),
                        frame.Frame.Width, frame.Frame.Height, false, Transparency);



                }
            }
        }
        public void DrawTileGroup(Methods.Draw.GraphicsHandler d, int x, int y, int x2, int y2, int height, int width, int Transperncy, SceneEntity entity, Controls.Editor.MainEditor EditorInstance)
        {

            Rectangle rect = GetTileArea(x2, y2, width, height);

            try
            {
                for (int ty = rect.Y; ty < rect.Y + rect.Height; ++ty)
                {
                    for (int tx = rect.X; tx < rect.X + rect.Width; ++tx)
                    {
                        // We will draw those later
                        if (this._layer.Tiles.Length <= ty)
                        {
                            //Skip
                        }
                        else if (this._layer.Tiles[ty].Length <= tx)
                        {
                            //Skip
                        }
                        else if (this._layer.Tiles?[ty][tx] != 0xffff)
                        {
                            DrawTile(d, this._layer.Tiles[ty][tx], (x) + tx - x2, (y) + ty - y2, false, Transperncy, EditorInstance);
                        }


                    }
                }
            }
            catch
            {

            }

        }

        public void DrawTile(Methods.Draw.GraphicsHandler d, ushort tile, int x, int y, bool selected, int Transperncy, Controls.Editor.MainEditor EditorInstance)
        {
            bool flipX = ((tile >> 10) & 1) == 1;
            bool flipY = ((tile >> 11) & 1) == 1;
            d.DrawBitmap(Methods.Editor.Solution.CurrentTiles.Image.GetTexture(d.DevicePanel._device, new Rectangle(0, (tile & 0x3ff) * TILE_SIZE, TILE_SIZE, TILE_SIZE), flipX, flipY),
            x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE, selected, Transperncy);
        }

        private Rectangle GetTileArea(int x, int y, int width, int height)
        {
            int y_start = y * height;
            int y_end = Math.Min((y + 1) * height, _layer.Height);

            int x_start = x * width;
            int x_end = Math.Min((x + 1) * width, _layer.Width);

            return new Rectangle(x, y, x + width, y + height);
        }

        public override bool isObjectOnScreen(Methods.Draw.GraphicsHandler d, SceneEntity entity, Classes.Scene.Sets.EditorEntity e, int x, int y, int Transparency)
        {
            int width = (int)entity.attributesMap["size"].ValueVector2.X.High;
            int height = (int)entity.attributesMap["size"].ValueVector2.Y.High;
            int x2 = (int)entity.attributesMap["offset"].ValueVector2.X.High;
            int y2 = (int)entity.attributesMap["offset"].ValueVector2.Y.High;

            int boundsX = width * 16;
            int boundsY = height * 16;

            return d.DevicePanel.IsObjectOnScreen(x - boundsX / 2, y - boundsY / 2, boundsX, boundsY);
        }

        public override string GetObjectName()
        {
            return "EncoreRoute";
        }
    }
}