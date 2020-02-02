﻿using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class TeeterTotter : EntityRenderer
    {

        public override void Draw(Structures.EntityLoadOptions properties)
        {
            Classes.Core.Draw.GraphicsHandler d = properties.Graphics;
            SceneEntity entity = properties.Object; 
            Classes.Core.Scene.Sets.EditorEntity e = properties.EditorObject;
            int x = properties.X;
            int y = properties.Y;
            int Transparency = properties.Transparency;
            int index = properties.Index;
            int previousChildCount = properties.PreviousChildCount;
            int platformAngle = properties.PlatformAngle;
            EditorAnimations Animation = properties.Animations;
            bool selected  = properties.isSelected;
            var value = entity.attributesMap["length"].ValueUInt32;
            var editorAnim = Controls.Base.MainEditor.Instance.EntityDrawing.LoadAnimation2("TeeterTotter", d.DevicePanel, 0, 0, false, false, false);
            if (editorAnim != null && editorAnim.Frames.Count != 0)
            {
                var frame = editorAnim.Frames[0];
                for (int i = -(int)value; i < value; ++i)
                {
                    d.DrawBitmap(new Classes.Core.Draw.GraphicsHandler.GraphicsInfo(frame), x + frame.Frame.PivotX + (i * (frame.Frame.Width + 2)),
                        y + frame.Frame.PivotY, frame.Frame.Width, frame.Frame.Height, false, Transparency);
                }
            }
        }

        public override string GetObjectName()
        {
            return "TeeterTotter";
        }
    }
}
