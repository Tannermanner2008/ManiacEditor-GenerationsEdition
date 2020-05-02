﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ManiacEditor.Methods.Draw
{
    public static class GraphicsTileDrawing
    {
        private static ManiacEditor.Controls.Editor.MainEditor Instance { get; set; }

        private static int TILE_SIZE
        {
            get
            {
                return Methods.Solution.SolutionConstants.TILE_SIZE;
            }
        }

        public static void UpdateInstance(ManiacEditor.Controls.Editor.MainEditor _Instance) 
        {
            Instance = _Instance;
        }

        public static void DrawTile(Graphics g, ushort tile, int x, int y, bool SemiTransparent = false)
        {
            DrawTile(g, tile, x, y, false, SemiTransparent);
        }

        public static void DrawTile(Graphics g, ushort tile, int x, int y, bool ChunkDraw, bool SemiTransparent)
        {
            ushort TileIndex = (ushort)(tile & 0x3ff);
            int TileIndexInt = (int)TileIndex;
            bool flipX = ((tile >> 10) & 1) == 1;
            bool flipY = ((tile >> 11) & 1) == 1;
            bool SolidTopA = ((tile >> 12) & 1) == 1;
            bool SolidLrbA = ((tile >> 13) & 1) == 1;
            bool SolidTopB = ((tile >> 14) & 1) == 1;
            bool SolidLrbB = ((tile >> 15) & 1) == 1;


            int CollisionOpacity = (int)Instance.EditorToolbar.CollisionOpacitySlider.Value;
            var CollisionAllSolidColor = Methods.Solution.SolutionState.Main.CollisionAllSolid_Color;
            var CollisionLRDSolidColor = Methods.Solution.SolutionState.Main.CollisionLRDSolid_Color;
            var CollisionTopOnlyColor = Methods.Solution.SolutionState.Main.CollisionTopOnlySolid_Color;


            System.Drawing.Color AllSolid = System.Drawing.Color.FromArgb(CollisionOpacity, CollisionAllSolidColor.R, CollisionAllSolidColor.G, CollisionAllSolidColor.B);
            System.Drawing.Color LRDSolid = System.Drawing.Color.FromArgb(CollisionOpacity, CollisionLRDSolidColor.R, CollisionLRDSolidColor.G, CollisionLRDSolidColor.B);
            System.Drawing.Color TopOnlySolid = System.Drawing.Color.FromArgb(CollisionOpacity, CollisionTopOnlyColor.R, CollisionTopOnlyColor.G, CollisionTopOnlyColor.B);

            g.DrawImage(Methods.Solution.CurrentSolution.CurrentTiles.Image.GetBitmap(new Rectangle(0, TileIndex * TILE_SIZE, TILE_SIZE, TILE_SIZE), flipX, flipY, SemiTransparent), new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE));

            if (ChunkDraw) return;

            if (Methods.Solution.SolutionState.Main.ShowCollisionA)
            {
                if (SolidLrbA || SolidTopA)
                {
                    if (SolidTopA && SolidLrbA) DrawCollision(true, AllSolid);
                    if (SolidTopA && !SolidLrbA) DrawCollision(true, TopOnlySolid);
                    if (SolidLrbA && !SolidTopA) DrawCollision(true, LRDSolid);
                }
            }
            if (Methods.Solution.SolutionState.Main.ShowCollisionB)
            {
                if (SolidLrbB || SolidTopB)
                {
                    if (SolidTopB && SolidLrbB) DrawCollision(false, AllSolid);
                    if (SolidTopB && !SolidLrbB) DrawCollision(false, TopOnlySolid);
                    if (SolidLrbB && !SolidTopB) DrawCollision(false, LRDSolid);
                }
            }

            if (Methods.Solution.SolutionState.Main.ShowFlippedTileHelper == true)
            {
                g.DrawImage(Methods.Solution.CurrentSolution.CurrentTiles.EditorImage.GetBitmap(new Rectangle(0, 3 * TILE_SIZE, TILE_SIZE, TILE_SIZE), false, false, SemiTransparent),
                            new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE));
            }
            if (Methods.Solution.SolutionState.Main.ShowTileID == true)
            {
                g.DrawImage(Methods.Solution.CurrentSolution.CurrentTiles.IDImage.GetBitmap(new Rectangle(0, TileIndex * TILE_SIZE, TILE_SIZE, TILE_SIZE), false, false, SemiTransparent),
                            new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE));
            }

            void DrawCollision(bool drawA, System.Drawing.Color colur)
            {
                Bitmap Map;
                if (drawA) Map = Methods.Solution.CurrentSolution.CurrentTiles.CollisionMaskA.GetBitmap(new Rectangle(0, (tile & 0x3ff) * TILE_SIZE, TILE_SIZE, TILE_SIZE), flipX, flipY, SemiTransparent);
                else Map = Methods.Solution.CurrentSolution.CurrentTiles.CollisionMaskB.GetBitmap(new Rectangle(0, (tile & 0x3ff) * TILE_SIZE, TILE_SIZE, TILE_SIZE), flipX, flipY, SemiTransparent);

                Map = Extensions.Extensions.ChangeImageColor(Map, System.Drawing.Color.White, colur);

                g.DrawImage(Map, x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);
            }


        }
    }
}