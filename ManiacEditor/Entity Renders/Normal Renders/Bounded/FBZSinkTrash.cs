﻿using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class FBZSinkTrash : EntityRenderer
    {

        public override void Draw(Structures.EntityRenderProp Properties)
        {
            DevicePanel d = Properties.Graphics;

            Classes.Scene.EditorEntity e = Properties.EditorObject;
            int x = Properties.DrawX;
            int y = Properties.DrawY;
            int Transparency = Properties.Transparency;

            var type = e.attributesMap["type"].ValueEnum;
            var widthPixels = (int)(e.attributesMap["size"].ValueVector2.X.High);
            var heightPixels = (int)(e.attributesMap["size"].ValueVector2.Y.High);

            if (widthPixels >= 1 && heightPixels >= 1)
            {
                d.DrawRectangle(x - widthPixels / 2, y - heightPixels / 2, x + widthPixels / 2, y + heightPixels / 2, System.Drawing.Color.Gray, System.Drawing.Color.White, 1);
            }
        }

        public override string GetObjectName()
        {
            return "FBZSinkTrash";
        }
    }
}
