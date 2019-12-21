﻿using RSDKv5;
using SystemColors = System.Drawing.Color;

namespace ManiacEditor.Entity_Renders
{
    public class LRZSpiral : EntityRenderer
    {

        public override void Draw(GraphicsHandler d, SceneEntity entity, EditorEntity e, int x, int y, int Transparency, int index = 0, int previousChildCount = 0, int platformAngle = 0, EditorAnimations Animation = null, bool selected = false, AttributeValidater attribMap = null)
        {
            int type = (int)entity.attributesMap["type"].ValueUInt8;
            int multiplierX = 0;
            int multiplierY = 0;
            switch (type)
            {
                case 0:
                    multiplierX = 2;
                    multiplierY = 128;
                    break;
                case 1:
                    multiplierX = 2;
                    multiplierY = 2;
                    break;
                case 2:
                    multiplierX = 3;
                    multiplierY = 3;
                    break;
            }
            var widthPixels = (int)(entity.attributesMap["radius"].ValueEnum) * multiplierX;
            var heightPixels = (int)(entity.attributesMap["height"].ValueEnum) * multiplierY;
            var width = (int)widthPixels / 16;
            var height = (int)heightPixels / 16;

            var editorAnim = Editor.Instance.EntityDrawing.LoadAnimation2("EditorAssets", d.DevicePanel, 0, 1, false, false, false);

            if (width != 0 && height != 0)
            {
                int x1 = x + widthPixels / -2;
                int x2 = x + widthPixels / 2 - 1;
                int y1 = y + heightPixels / -2;
                int y2 = y + heightPixels / 2 - 1;


                d.DrawLine(x1, y1, x1, y2, SystemColors.White);
                d.DrawLine(x1, y1, x2, y1, SystemColors.White);
                d.DrawLine(x2, y2, x1, y2, SystemColors.White);
                d.DrawLine(x2, y2, x2, y1, SystemColors.White);

                // draw corners
                for (int i = 0; i < 4; i++)
                {
                    bool right = (i & 1) > 0;
                    bool bottom = (i & 2) > 0;

                    editorAnim = Editor.Instance.EntityDrawing.LoadAnimation2("EditorAssets", d.DevicePanel, 0, 1, right, bottom, false);
                    if (editorAnim != null && editorAnim.Frames.Count != 0)
                    {
                        var frame = editorAnim.Frames[Animation.index];
                        Animation.ProcessAnimation(frame.Entry.SpeedMultiplyer, frame.Entry.Frames.Count, frame.Frame.Delay);
                        d.DrawBitmap(new GraphicsHandler.GraphicsInfo(frame),
                            (x + widthPixels / (right ? 2 : -2)) - (right ? frame.Frame.Width : 0),
                            (y + heightPixels / (bottom ? 2 : -2) - (bottom ? frame.Frame.Height : 0)),
                            frame.Frame.Width, frame.Frame.Height, false, Transparency);

                    }
                }
            }
        }

        public override bool isObjectOnScreen(GraphicsHandler d, SceneEntity entity, EditorEntity e, int x, int y, int Transparency)
        {
            int type = (int)entity.attributesMap["type"].ValueUInt8;
            int multiplierX = 0;
            int multiplierY = 0;
            switch (type)
            {
                case 0:
                    multiplierX = 2;
                    multiplierY = 128;
                    break;
                case 1:
                    multiplierX = 2;
                    multiplierY = 2;
                    break;
                case 2:
                    multiplierX = 3;
                    multiplierY = 3;
                    break;
            }
            var widthPixels = (int)(entity.attributesMap["radius"].ValueEnum) * multiplierX;
            var heightPixels = (int)(entity.attributesMap["height"].ValueEnum) * multiplierY;
            if (widthPixels != 0 && heightPixels != 0)
            {
                return d.IsObjectOnScreen(x - widthPixels / 2, y - heightPixels / 2, widthPixels, heightPixels);
            }
            else
            {
                return d.IsObjectOnScreen(x - 16, y - 16, 32, 32);
            }

        }

        public override string GetObjectName()
        {
            return "LRZSpiral";
        }
    }
}