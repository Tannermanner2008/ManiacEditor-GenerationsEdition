﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiacEditor
{
    class LinkedEditorEntity : EditorEntity
    {
        private uint goProperty;
        private uint destinationTag;
        private byte tag;
        private ushort slotID;
        private ushort targetSlotID;
        private RSDKv5.SceneEntity currentEntity;
        private Editor EditorInstance2;

		private byte TransportTubeType;

        public LinkedEditorEntity(RSDKv5.SceneEntity entity, Editor instance) : base(entity, instance)
        {
            EditorInstance2 = instance;
            if (entity.Object.Name.Name == "WarpDoor")
            {
                goProperty = Entity.GetAttribute("go").ValueVar;
                destinationTag = Entity.GetAttribute("destinationTag").ValueVar;
                tag = Entity.GetAttribute("tag").ValueUInt8;
                currentEntity = Entity;
            }
            else if (entity.Object.Name.Name == "TornadoPath")
            {
                slotID = Entity.SlotID;
                targetSlotID = (ushort)(Entity.SlotID + 1);
                currentEntity = Entity;
            }
			else if (entity.Object.Name.Name == "TransportTube")
			{
				TransportTubeType = Entity.GetAttribute("type").ValueUInt8;
				slotID = Entity.SlotID;
				targetSlotID = (ushort)(Entity.SlotID + 1);
				currentEntity = Entity;
			}
            else if (entity.Object.Name.Name == "AIZTornadoPath")
            {
                slotID = Entity.SlotID;
                targetSlotID = (ushort)(Entity.SlotID + 1);
                currentEntity = Entity;
            }

        }

        public override void Draw(DevicePanel d)
        {
			if (EditorInstance.showEntityPathArrows)
            {
                if (currentEntity.Object.Name.Name == "WarpDoor")
                {
                    base.Draw(d);
                    if (goProperty == 1 && destinationTag == 0) return; // probably just a destination

                    // this is the start of a WarpDoor, find its partner(s)
                    var warpDoors = Entity.Object.Entities.Where(e => e.GetAttribute("tag").ValueUInt8 ==
                                                                        destinationTag);

                    if (warpDoors != null
                        && warpDoors.Any())
                    {
                        // some destinations seem to be duplicated, so we must loop
                        foreach (var wd in warpDoors)
                        {
                            DrawLinkArrow(d, Entity, wd);
                        }
                    }
                }
				else if (currentEntity.Object.Name.Name == "TornadoPath")
                {
                    base.Draw(d);

                    //if (goProperty == 1 && destinationTag == 0) return; // probably just a destination

                    // this is the start of a WarpDoor, find its partner(s)
                    var tornadoPaths = Entity.Object.Entities.Where(e => e.SlotID == targetSlotID);

                    if (tornadoPaths != null
                        && tornadoPaths.Any())
                    {
                        // some destinations seem to be duplicated, so we must loop
                        foreach (var tp in tornadoPaths)
                        {
                            DrawLinkArrow(d, Entity, tp);
                        }
                    }
                }
                else if (currentEntity.Object.Name.Name == "AIZTornadoPath")
                {
                    base.Draw(d);

                    //if (goProperty == 1 && destinationTag == 0) return; // probably just a destination

                    // this is the start of a WarpDoor, find its partner(s)
                    var tornadoPaths = Entity.Object.Entities.Where(e => e.SlotID == targetSlotID);

                    if (tornadoPaths != null
                        && tornadoPaths.Any())
                    {
                        // some destinations seem to be duplicated, so we must loop
                        foreach (var tp in tornadoPaths)
                        {
                            DrawLinkArrow(d, Entity, tp);
                        }
                    }
                }
            }

			if (currentEntity.Object.Name.Name == "TransportTube")
			{
				if (EditorInstance.showEntityPathArrows)
				{
					if ((TransportTubeType == 2 || TransportTubeType == 4))
					{
						var transportTubePaths = Entity.Object.Entities.Where(e => e.SlotID == targetSlotID);

						if (transportTubePaths != null && transportTubePaths.Any())
						{
							foreach (var ttp in transportTubePaths)
							{
								int destinationType = ttp.GetAttribute("type").ValueUInt8;
								if (destinationType == 3)
								{
									DrawLinkArrowTransportTubes(d, Entity, ttp, 3, TransportTubeType);
								}
								else if (destinationType == 4)
								{
									DrawLinkArrowTransportTubes(d, Entity, ttp, 4, TransportTubeType);
								}
								else if (destinationType == 2)
								{
									DrawLinkArrowTransportTubes(d, Entity, ttp, 2, TransportTubeType);
								}
								else
								{
									DrawLinkArrowTransportTubes(d, Entity, ttp, 1, TransportTubeType);
								}

							}
						}
					}
				}

				base.Draw(d);
			}
		}

        private void DrawLinkArrow(DevicePanel d, RSDKv5.SceneEntity start, RSDKv5.SceneEntity end)
        {
            int startX = start.Position.X.High;
            int startY = start.Position.Y.High;
            int endX = end.Position.X.High;
            int endY = end.Position.Y.High;

            int dx = endX - startX;
            int dy = endY - startY;

            int offsetX = 0;
            int offsetY = 0;
            int offsetDestinationX = 0;
            int offsetDestinationY = 0;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                // horizontal difference greater than vertical difference
                offsetY = NAME_BOX_HALF_HEIGHT;
                offsetDestinationY = NAME_BOX_HALF_HEIGHT;

                if (dx > 0)
                {
                    offsetX = NAME_BOX_WIDTH;
                }
                else
                {
                    offsetDestinationX = NAME_BOX_WIDTH;
                }
            }
            else
            {
                // vertical difference greater than horizontal difference
                offsetX = NAME_BOX_HALF_WIDTH;
                offsetDestinationX = NAME_BOX_HALF_WIDTH;

                if (dy > 0)
                {
                    offsetY = NAME_BOX_HEIGHT;
                }
                else
                {
                    offsetDestinationY = NAME_BOX_HEIGHT;
                }
            }

            d.DrawArrow(startX + offsetX,
                        startY + offsetY,
                        end.Position.X.High + offsetDestinationX,
                        end.Position.Y.High + offsetDestinationY,
                        Color.GreenYellow);
        }

		private void DrawLinkArrowTransportTubes(DevicePanel d, RSDKv5.SceneEntity start, RSDKv5.SceneEntity end, int destType, int sourceType)
		{
			Color color = Color.Transparent;
			switch (destType)
			{
				case 4:
					color = Color.Yellow;
					break;
				case 3:
					color = Color.Red;
					break;
			}
			if (sourceType == 2)
			{
				switch (destType)
				{
					case 4:
						color = Color.Green;
						break;
					case 3:
						color = Color.Red;
						break;
				}
			}
			int startX = start.Position.X.High;
			int startY = start.Position.Y.High;
			int endX = end.Position.X.High;
			int endY = end.Position.Y.High;

			int dx = endX - startX;
			int dy = endY - startY;

			int offsetX = 0;
			int offsetY = 0;
			int offsetDestinationX = 0;
			int offsetDestinationY = 0;

			d.DrawArrow(startX + offsetX,
						startY + offsetY,
						end.Position.X.High + offsetDestinationX,
						end.Position.Y.High + offsetDestinationY,
						color);
		}
	}
}