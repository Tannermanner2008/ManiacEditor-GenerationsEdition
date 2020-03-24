﻿using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class BouncePlant : EntityRenderer
    {

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
            int direction = (int)entity.attributesMap["direction"].ValueUInt8;
            bool fliph = false;
            bool flipv = false;
            if (direction == 0)
            {
                fliph = true;
            }
            var editorAnim = Controls.Editor.MainEditor.Instance.EntityDrawing.LoadAnimation2("Plants", d.DevicePanel, 1, -1, fliph, flipv, false);
            x += (fliph ? 42 : -42);
            y += -42;
            if (editorAnim != null && editorAnim.Frames.Count != 0)
            {
                var frame = editorAnim.Frames[0];

                for (int i = 0; i < 8; i++)
                {
                    d.DrawBitmap(new Methods.Draw.GraphicsHandler.GraphicsInfo(frame),
                        x + frame.Frame.PivotX + (fliph ? -(12 * i) : (12 * i)),
                        y + frame.Frame.PivotY + 12*i,
                        frame.Frame.Width, frame.Frame.Height, false, Transparency);
                }

            }
        }

        public override string GetObjectName()
        {
            return "BouncePlant";
        }
    }
}