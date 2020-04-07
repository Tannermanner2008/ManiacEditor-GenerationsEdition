﻿using RSDKv5;
using SystemColors = System.Drawing.Color;

namespace ManiacEditor.Entity_Renders
{
    public class CorkscrewPath : EntityRenderer
    {

        public override void Draw(Structures.EntityRenderProp Properties)
        {
            DevicePanel d = Properties.Graphics;

            Classes.Scene.EditorEntity e = Properties.EditorObject;
            int x = Properties.DrawX;
            int y = Properties.DrawY;
            int Transparency = Properties.Transparency;

            var period = (int)(e.attributesMap["period"].ValueEnum);
            var amplitude = (int)(e.attributesMap["amplitude"].ValueEnum * 3.5);
            var width = (int)period / 16;
            var height = (int)amplitude / 16;
            var Animation = LoadAnimation("EditorIcons", d, 0, 4);
            DrawTexturePivotNormal(d, Animation, Animation.RequestedAnimID, Animation.RequestedFrameID, x, y, Transparency, false, false);
            DrawBounds(d, x, y, period, amplitude, Transparency, SystemColors.White, SystemColors.Transparent);
        }

        public override string GetObjectName()
        {
            return "CorkscrewPath";
        }
    }
}
