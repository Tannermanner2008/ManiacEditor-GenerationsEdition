﻿using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class Chopper : EntityRenderer
    {

        public override void Draw(GraphicsHandler d, SceneEntity entity, Classes.Editor.Scene.Sets.EditorEntity e, int x, int y, int Transparency, int index = 0, int previousChildCount = 0, int platformAngle = 0, EditorAnimations Animation = null, bool selected = false, AttributeValidater attribMap = null)
        {
            int type = (int)entity.attributesMap["type"].ValueUInt8;
            int direction = (int)entity.attributesMap["direction"].ValueUInt8;
            bool charge = entity.attributesMap["charge"].ValueBool;
            bool fliph = false;
            bool flipv = false;
            int animID;
            if (type == 1)
            {
                if (charge == true)
                {
                    animID = 3;
                }
                else
                {
                    animID = 1;
                }

            }
            else
            {
                animID = 0;
            }
            if (direction == 1)
            {
                fliph = true;
            }
            if (direction == 1)
            {
                fliph = true;
            }
            var editorAnim = Interfaces.Base.MainEditor.Instance.EntityDrawing.LoadAnimation2("Chopper", d.DevicePanel, animID, -1, fliph, flipv, false);
            if (editorAnim != null && editorAnim.Frames.Count != 0 && animID >= 0)
            {
                var frame = editorAnim.Frames[Animation.index];

                Animation.ProcessAnimation(frame.Entry.SpeedMultiplyer, frame.Entry.Frames.Count, frame.Frame.Delay);

                d.DrawBitmap(new GraphicsHandler.GraphicsInfo(frame),
                    x + frame.Frame.PivotX - (fliph ? (frame.Frame.Width - editorAnim.Frames[0].Frame.Width) : 0),
                    y + frame.Frame.PivotY + (flipv ? (frame.Frame.Height - editorAnim.Frames[0].Frame.Height) : 0),
                    frame.Frame.Width, frame.Frame.Height, false, Transparency);
            }
        }

        public override string GetObjectName()
        {
            return "Chopper";
        }
    }
}
