﻿using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class UICharButton : EntityRenderer
    {

        public override void Draw(Structures.EntityRenderProp properties)
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
            Methods.Entities.EntityAnimator Animation = properties.Animations;
            bool selected  = properties.isSelected;

            int characterID = (int)entity.attributesMap["characterID"].ValueUInt8;
            int characterID_text = characterID;
            if (characterID >= 3) characterID++;
            string text = "Text" + Classes.Core.SolutionState.CurrentLanguage;
            var editorAnim = Controls.Base.MainEditor.Instance.EntityDrawing.LoadAnimation(text, d.DevicePanel, 8, characterID_text, false, false, false);
            var editorAnimFrame = Controls.Base.MainEditor.Instance.EntityDrawing.LoadAnimation("EditorUIRender", d.DevicePanel, 1, 1, false, false, false);
            var editorAnimIcon = Controls.Base.MainEditor.Instance.EntityDrawing.LoadAnimation("SaveSelect", d.DevicePanel, 1, characterID, false, false, false);

            d.DrawRectangle(x - 48, y - 48, x + 48, y + 48, System.Drawing.Color.FromArgb(128, 255, 255, 255));

            if (editorAnimFrame != null && editorAnimFrame.Frames.Count != 0)
            {
                var frame = editorAnimFrame.Frames[Animation.index];
                d.DrawBitmap(new Classes.Core.Draw.GraphicsHandler.GraphicsInfo(frame), x + frame.Frame.PivotX, y + frame.Frame.PivotY,
                    frame.Frame.Width, frame.Frame.Height, false, Transparency);
            }
            if (editorAnim != null && editorAnim.Frames.Count != 0)
            {
                var frame = editorAnim.Frames[Animation.index];
                d.DrawBitmap(new Classes.Core.Draw.GraphicsHandler.GraphicsInfo(frame), x + frame.Frame.PivotX, y + frame.Frame.PivotY + 32,
                    frame.Frame.Width, frame.Frame.Height, false, Transparency);
            }
            if (editorAnimIcon != null && editorAnimIcon.Frames.Count != 0)
            {
                var frame = editorAnimIcon.Frames[Animation.index];
                d.DrawBitmap(new Classes.Core.Draw.GraphicsHandler.GraphicsInfo(frame), x + frame.Frame.PivotX, y + frame.Frame.PivotY - 8,
                    frame.Frame.Width, frame.Frame.Height, false, Transparency);

            }



        }

        public override string GetObjectName()
        {
            return "UICharButton";
        }
    }
}
