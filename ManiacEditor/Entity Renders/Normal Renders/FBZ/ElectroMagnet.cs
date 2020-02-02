﻿using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class ElectroMagnet : EntityRenderer
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
            bool fliph = false;
            bool flipv = false;
            bool invisible = entity.attributesMap["invisible"].ValueBool;
            var editorAnim = Controls.Base.MainEditor.Instance.EntityDrawing.LoadAnimation2("ElectroMagnet", d.DevicePanel, 0, 0, fliph, flipv, false);
            if (editorAnim != null && editorAnim.Frames.Count != 0 && invisible == false)
            {
                var frame = editorAnim.Frames[0];
                // Animation.ProcessAnimation(frame.Entry.SpeedMultiplyer, frame.Entry.Frames.Count, frame.Frame.Delay);
                    d.DrawBitmap(new Classes.Core.Draw.GraphicsHandler.GraphicsInfo(frame),
                        x + frame.Frame.PivotX + (fliph ? (frame.Frame.Width - editorAnim.Frames[0].Frame.Width * 2) : 0),
                        y + frame.Frame.PivotY + (flipv ? (frame.Frame.Height - editorAnim.Frames[0].Frame.Height) : 0),
                        frame.Frame.Width, frame.Frame.Height, false, Transparency);
            }
        }

        public override string GetObjectName()
        {
            return "ElectroMagnet";
        }
    }
}
