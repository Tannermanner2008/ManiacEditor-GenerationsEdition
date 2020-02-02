﻿using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class UIButton : EntityRenderer
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
            string text = "Text" + Classes.Core.SolutionState.CurrentLanguage;
            int frameID = (int)entity.attributesMap["frameID"].ValueEnum;
            int listID = (int)entity.attributesMap["listID"].ValueEnum;
            int align = (int)entity.attributesMap["align"].ValueEnum;
            int width = (int)entity.attributesMap["size"].ValueVector2.X.High;
            int height = (int)entity.attributesMap["size"].ValueVector2.Y.High;
            bool invisible = entity.attributesMap["invisible"].ValueBool;
            double alignmentVal = 0;
            var editorAnim = Controls.Base.MainEditor.Instance.EntityDrawing.LoadAnimation(text, d.DevicePanel, listID, frameID, false, false, false);
            if (editorAnim != null && editorAnim.Frames.Count != 0 && !invisible)
            {
                var frame = editorAnim.Frames[0];
                switch (align)
                {
                    case 0:
                        alignmentVal = -((width / 2)) - frame.Frame.PivotY;
                        break;
                    default:
                        alignmentVal = frame.Frame.PivotX + (22 / 2);
                        break;
                }
                e.DrawUIButtonBack(d, x, y, width, height, frame.Frame.Width, frame.Frame.Height, Transparency);
                d.DrawBitmap(new Classes.Core.Draw.GraphicsHandler.GraphicsInfo(frame), x + (int)alignmentVal, y + frame.Frame.PivotY,
                    frame.Frame.Width, frame.Frame.Height, false, Transparency);
            }


        }

        public override string GetObjectName()
        {
            return "UIButton";
        }
    }
}
