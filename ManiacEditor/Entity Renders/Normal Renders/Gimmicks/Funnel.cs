﻿using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class Funnel : EntityRenderer
    {

        public override void Draw(Structures.EntityRenderProp Properties)
        {
            DevicePanel d = Properties.Graphics;

            Classes.Scene.EditorEntity e = Properties.EditorObject;
            int x = Properties.DrawX;
            int y = Properties.DrawY;
            int Transparency = Properties.Transparency;

            bool fliph = false;
            bool flipv = false;

            var Animation = LoadAnimation("Funnel", d, 0, 0);
            DrawTexturePivotNormal(d, Animation, Animation.RequestedAnimID, Animation.RequestedFrameID, x, y, Transparency, fliph, flipv);
            Animation = LoadAnimation("Funnel", d, 0, 1);
            DrawTexturePivotNormal(d, Animation, Animation.RequestedAnimID, Animation.RequestedFrameID, x, y, Transparency, fliph, flipv);
        }

        public override string GetObjectName()
        {
            return "Funnel";
        }
    }
}