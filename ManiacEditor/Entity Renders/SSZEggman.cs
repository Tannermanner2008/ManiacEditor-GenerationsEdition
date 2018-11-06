﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ManiacEditor;
using Microsoft.Xna.Framework;
using RSDKv5;

namespace ManiacEditor.Entity_Renders
{
    public class SSZEggman : EntityRenderer
    {

        public override void Draw(DevicePanel d, SceneEntity entity, EditorEntity e, int x, int y, int Transparency)
        {
            var editorAnim = e.LoadAnimation2("EggmanSSZ", d, 0, -1, false, false, false);
            var editorAnimMobile = e.LoadAnimation2("EggmanSSZ", d, 5, -1, false, false, false);
            var editorAnimSeat = e.LoadAnimation2("EggmanSSZ", d, 4, -1, false, false, false);
            if (editorAnim != null && editorAnim.Frames.Count != 0 && editorAnimMobile != null && editorAnimMobile.Frames.Count != 0 && editorAnimSeat != null && editorAnimSeat.Frames.Count != 0)
            {
                var frame = editorAnim.Frames[e.index];
                var frameMobile = editorAnimMobile.Frames[0];
                var frameSeat = editorAnimSeat.Frames[0];

                e.ProcessAnimation(frame.Entry.FrameSpeed, frame.Entry.Frames.Count, frame.Frame.Duration);

                d.DrawBitmap(frameSeat.Texture,
                    x + frameSeat.Frame.CenterX,
                    y + frameSeat.Frame.CenterY,
                    frameSeat.Frame.Width, frameSeat.Frame.Height, false, Transparency);
                d.DrawBitmap(frame.Texture,
                    x + frame.Frame.CenterX,
                    y + frame.Frame.CenterY,
                    frame.Frame.Width, frame.Frame.Height, false, Transparency);
                d.DrawBitmap(frameMobile.Texture,
                    x + frameMobile.Frame.CenterX,
                    y + frameMobile.Frame.CenterY,
                    frameMobile.Frame.Width, frameMobile.Frame.Height, false, Transparency);
            }
        }

        public override string GetObjectName()
        {
            return "SSZEggman";
        }
    }
}
