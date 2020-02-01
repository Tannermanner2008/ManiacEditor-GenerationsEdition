﻿using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class TimeAttackGate : EntityRenderer
    {

        public override void Draw(GraphicsHandler d, SceneEntity entity, Classes.Editor.Scene.Sets.EditorEntity e, int x, int y, int Transparency, int index = 0, int previousChildCount = 0, int platformAngle = 0, EditorAnimations Animation = null, bool selected = false, AttributeValidater attribMap = null)
        {
            bool finish = entity.attributesMap["finishLine"].ValueBool;
            var editorAnimBase = Interfaces.Base.MainEditor.Instance.EntityDrawing.LoadAnimation2("SpeedGate", d.DevicePanel, 0, 0, false, false, false);
            var editorAnimTop = Interfaces.Base.MainEditor.Instance.EntityDrawing.LoadAnimation2("SpeedGate", d.DevicePanel, 1, 0, false, false, false);
            var editorAnimFins = Interfaces.Base.MainEditor.Instance.EntityDrawing.LoadAnimation2("SpeedGate", d.DevicePanel, finish ? 4 : 3, -1, false, false, false);
            if (editorAnimBase != null && editorAnimTop != null && editorAnimFins != null && editorAnimFins.Frames.Count != 0 && editorAnimTop.Frames.Count != 0 && editorAnimTop.Frames.Count != 0)
            {
                var frameBase = editorAnimBase.Frames[0];
                var frameTop = editorAnimTop.Frames[0];
                d.DrawBitmap(new GraphicsHandler.GraphicsInfo(frameBase), x + frameBase.Frame.PivotX, y + frameBase.Frame.PivotY,
                    frameBase.Frame.Width, frameBase.Frame.Height, false, Transparency);
                d.DrawBitmap(new GraphicsHandler.GraphicsInfo(frameTop), x + frameTop.Frame.PivotX, y + frameTop.Frame.PivotY,
                    frameTop.Frame.Width, frameTop.Frame.Height, false, Transparency);
                for (int i = 0; i < editorAnimFins.Frames.Count; ++i)
                {
                    var frame = editorAnimFins.Frames[i];
                    d.DrawBitmap(new GraphicsHandler.GraphicsInfo(frame), x + frame.Frame.PivotX, y + frame.Frame.PivotY,
                        frame.Frame.Width, frame.Frame.Height, false, Transparency);
                }
            }
        }

        public override string GetObjectName()
        {
            return "TimeAttackGate";
        }
    }
}
