﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using IronPython.Modules;
using RSDKv5;
using SharpDX.Direct3D9;
using ManiacEditor.Entity_Renders;

namespace ManiacEditor
{
    public class EditorEntities : IDrawable
    {
        protected const int NAME_BOX_WIDTH = 20;
        protected const int NAME_BOX_HEIGHT = 20;

        public bool FilterRefreshNeeded = false;
        public int DefaultFilter = -1;

        public bool OptimizeAssets = false;

        public List<EditorEntity> entities = new List<EditorEntity>();
        public List<EditorEntity> selectedEntities = new List<EditorEntity>();
        public List<EditorEntity> tempSelection = new List<EditorEntity>();


        Dictionary<ushort, EditorEntity> entitiesBySlot = new Dictionary<ushort, EditorEntity>();

        ushort nextFreeSlot = 0;

        public List<EditorEntity> Entities { get { return entities; } }
        public List<EditorEntity> SelectedEntities { get { return selectedEntities; } }

        public class TooManyEntitiesException : Exception { }

        public Actions.IAction LastAction;

        public int layerPrority = 0;

        public string SetupObject = "";

        public Editor EditorInstance;

        public EditorEntities(Scene scene, Editor instance)
        {
            EditorInstance = instance;
            foreach (var obj in scene.Objects)
            {
                entities.AddRange(obj.Entities.Select(x => GenerateEditorEntity(x)));
            }
            FindDuplicateIds();
            SetupObject = GetSetupObject(scene);
            entitiesBySlot = entities.ToDictionary(x => x.Entity.SlotID);
        }

        private void FindDuplicateIds()
        {
            var groupedById = entities.GroupBy(e => e.Entity.SlotID).Where(g => g.Count() > 1);
        }

        public List<string> GetObjects(List<RSDKv5.SceneObject> sceneObjects)
        {
            sceneObjects.Sort((x, y) => x.Name.ToString().CompareTo(y.Name.ToString()));
            List<string> strings = sceneObjects.Select(s => s.Name.Name).ToList();
            return strings;
        }

        public string GetSetupObject(Scene scene)
        {
            try
            {
                var objectList = GetObjects(EditorInstance.EditorScene.Objects);
                string setupObject = objectList.FirstOrDefault(x => x.Contains("Setup"));
                return setupObject;
            }
            catch
            {
                return "";
            }
        }

        private ushort getFreeSlot(RSDKv5.SceneEntity preferred)
        {
            if (preferred != null && !entitiesBySlot.ContainsKey(preferred.SlotID)) return preferred.SlotID;
            while (entitiesBySlot.ContainsKey(nextFreeSlot))
            {
                ++nextFreeSlot;
            }
            if (nextFreeSlot == 2048)
            {
                if (entitiesBySlot.Count < 2048)
                {
                    // Next time search from beggining
                    nextFreeSlot = 0;
                }
                throw new TooManyEntitiesException();
            }
            return nextFreeSlot++;
        }

		public void Select(Rectangle area, bool addSelection = false, bool deselectIfSelected = false)
        {
            if (!addSelection) Deselect();
            foreach (var entity in entities)
            {
                if (entity.IsInArea(area))
                {
                    if (deselectIfSelected && selectedEntities.Contains(entity))
                    {
                        selectedEntities.Remove(entity);
                        entity.Selected = false;
                    }
                    else if (!selectedEntities.Contains(entity))
                    {
                        selectedEntities.Add(entity);
                        entity.Selected = true;
                    }
                }
            }
        }

        public void Select(Point point, bool addSelection = false, bool deselectIfSelected = false)
        {
            if (!addSelection) Deselect();
            // In reverse because we want to select the top one
            foreach (EditorEntity entity in entities.Reverse<EditorEntity>())
            {
                if (entity.ContainsPoint(point))
                {
                    if (deselectIfSelected && selectedEntities.Contains(entity))
                    {
                        selectedEntities.Remove(entity);
                        entity.Selected = false;
                    }
                    else
                    {
                        selectedEntities.Add(entity);
                        entity.Selected = true;
                    }
                    // Only the top
                    break;
                }
            }
        }

        public void SelectSlot(int slot)
        {
            Deselect();
            if (entitiesBySlot.ContainsKey((ushort)slot))
            {
                selectedEntities.Add(entitiesBySlot[(ushort)slot]);
                entitiesBySlot[(ushort)slot].Selected = true;
            }
        }

        /// <summary>
        /// Adds an entity to the Scene, and consumes the specified ID Slot.
        /// </summary>
        /// <param name="entity">Entity to add to the scene.</param>
        private void AddEntity(EditorEntity entity)
        {
            entity.Entity.Object.Entities.Add(entity.Entity);
            this.entities.Add(entity);
            entitiesBySlot[entity.Entity.SlotID] = entity;
        }


        public void OptimizeSlotIDs()
        {
            var entitiesSortedBySlot = entities.OrderBy(e => e.Entity.SlotID).ToList();
            int lastSlotID = entitiesBySlot.Values.Last().Entity.SlotID;
            int unusedSpaces = lastSlotID - entitiesSortedBySlot.Count;
            for (int i = 0; i < entities.Count; i++)
            {
                entitiesSortedBySlot[i].Entity.SlotID = (ushort)i;
            }
            entities = entitiesSortedBySlot;
            //MessageBox.Show("There are exactly " + unusedSpaces + " unused slots, in this scene", "Results");
        }

        /// <summary>
        /// Adds a set of entities to the Scene, and consumes the ID Slot specified for each.
        /// </summary>
        /// <param name="entities">Set of entities.</param>
        private void AddEntities(IEnumerable<EditorEntity> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var entity in entities)
            {
                AddEntity(entity);
            }
}

        private void DeleteEntities(List<EditorEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.Entity.Object.Entities.Remove(entity.Entity);
                this.entities.Remove(entity);
                entitiesBySlot.Remove(entity.Entity.SlotID);
                if (entity.Entity.SlotID < nextFreeSlot) nextFreeSlot = entity.Entity.SlotID;
            }
        }

        private void DuplicateEntities(List<EditorEntity> entities)
        {
            if (null == entities || !entities.Any()) return;

            // work out a slot for each entity, and add it in turn
            // this should prevent generating the same ID for each member of the list
            var new_entities = new List<EditorEntity>();
            foreach (var entity in entities)
            {
                ushort slot = getFreeSlot(entity.Entity);

                SceneEntity sceneEntity;
                // If this is pasted from another Scene, we need to reassign its Object
                if (entity.IsExternal())
                    sceneEntity = SceneEntity.FromExternal(entity.Entity, EditorInstance.EditorScene.Objects, slot);
                // If it's from this Scene, we can use the existing Object
                else
                    sceneEntity = new SceneEntity(entity.Entity, slot);

                // Make sure it was created properly
                if (sceneEntity != null)
                {
                    var editorEntity = GenerateEditorEntity(sceneEntity);
                    AddEntity(editorEntity);
                    new_entities.Add(editorEntity);
                }
            }
            if (new_entities.Count > 0)
                LastAction = new Actions.ActionAddDeleteEntities(new_entities.ToList(), true, x => AddEntities(x), x => DeleteEntities(x));
            Deselect();
            selectedEntities.AddRange(new_entities);
            foreach (var entity in new_entities)
                entity.Selected = true;
        }

        public void MoveSelected(Point oldPos, Point newPos, bool duplicate)
        {
            Point diff = new Point(newPos.X - oldPos.X, newPos.Y - oldPos.Y);
            if (duplicate) DuplicateEntities(selectedEntities);
            foreach (var entity in selectedEntities)
            {
                entity.Move(diff);
            }
        }

        public bool IsSelected()
        {
            return selectedEntities.Count > 0 || tempSelection.Count > 0;
        }

        public void DeleteSelected()
        {
            if (selectedEntities.Count > 0)
                LastAction = new Actions.ActionAddDeleteEntities(selectedEntities.ToList(), false, x => AddEntities(x), x => DeleteEntities(x));
            DeleteEntities(selectedEntities);
            Deselect();
        }

        public List<EditorEntity> CopyToClipboard(bool keepPosition = false)
        {
            if (selectedEntities.Count == 0) return null;
            short minX = 0, minY = 0;

            List<EditorEntity> copiedEntities = selectedEntities.Select(x => GenerateEditorEntity(new RSDKv5.SceneEntity(x.Entity, x.Entity.SlotID))).ToList();
            if (!keepPosition)
            {
                minX = copiedEntities.Min(x => x.Entity.Position.X.High);
                minY = copiedEntities.Min(x => x.Entity.Position.Y.High);
                copiedEntities.ForEach(x => x.Move(new Point(-minX, -minY)));
            }

            return copiedEntities;
        }

        public void PasteFromClipboard(Point newPos, List<EditorEntity> entities)
        {
            DuplicateEntities(entities);
            foreach (var entity in selectedEntities)
            {
                // Move them
                entity.Move(newPos);
            }
        }

        public EditorEntity GetEntityAt(Point point)
        {
            foreach (EditorEntity entity in entities.Reverse<EditorEntity>())
                if (entity.ContainsPoint(point))
                    return entity;
            return null;
        }

        public void TempSelection(Rectangle area, bool deselectIfSelected)
        {
            List<EditorEntity> newSelection = (from entity in entities where entity.IsInArea(area) select entity).ToList();

            foreach (var entity in (from entity in tempSelection where !newSelection.Contains(entity) select entity))
            {
                entity.Selected = selectedEntities.Contains(entity);
            }

            tempSelection = newSelection;

            foreach (var entity in newSelection)
            {
                entity.Selected = !deselectIfSelected || !selectedEntities.Contains(entity);
            }
        }
        public void Deselect()
        {
            foreach (var entity in entities)
            {
                entity.Selected = false;
            }
            selectedEntities.Clear();
        }

        public void EndTempSelection()
        {
            tempSelection.Clear();
        }

        public void Draw(Graphics g)
        {

        }

        public void Draw(DevicePanel d)
        {
            if (FilterRefreshNeeded)
                UpdateViewFilters();
            foreach (var entity in entities)
            {
                if (entity.IsObjectOnScreen(d)) entity.Draw(d);
            }
        }

		public void DrawPriority(DevicePanel d, int prority)
        {
            if (FilterRefreshNeeded)
                UpdateViewFilters();
            foreach (var entity in entities)
            {
                if (entity.ValidPriorityPlane(prority) && entity.IsObjectOnScreen(d)) entity.Draw(d);
            }


        }

        /// <summary>
        /// Creates a new instance of the given SceneObject at the indicated position.
        /// </summary>
        /// <param name="sceneObject">Type of SceneObject to create an instance of.</param>
        /// <param name="position">Location to insert into the scene.</param>
        public void Add(RSDKv5.SceneObject sceneObject, RSDKv5.Position position)
        {
            var editorEntity = GenerateEditorEntity(new RSDKv5.SceneEntity(sceneObject, getFreeSlot(null)));
            editorEntity.Entity.Position = position;
            var newEntities = new List<EditorEntity> { editorEntity };
            LastAction = new Actions.ActionAddDeleteEntities(newEntities, true, x => AddEntities(x), x => DeleteEntities(x));
            AddEntities(newEntities);

            Deselect();
            editorEntity.Selected = true;
            selectedEntities.Add(editorEntity);
        }

        private EditorEntity GenerateEditorEntity(RSDKv5.SceneEntity sceneEntity)
        {
            /*
            try
            {
                string objectName = sceneEntity.Object.Name.Name;
                if (LinkedRendersNames.Contains(objectName)) return new LinkedEditorEntity(sceneEntity, EditorInstance);
            }
            catch
            {
                Debug.WriteLine("Failed to generate a LinkedEditorEntity, will create a basic one instead.");
            }*/

            EditorEntity entity = new EditorEntity(sceneEntity, EditorInstance);

            if (entity.HasFilter() && DefaultFilter > -1)
            {
                entity.Entity.GetAttribute("filter").ValueUInt8 = (byte)DefaultFilter;
                DefaultFilter = -1;
            }

            entity.SetFilter();

            return entity;
        }
        public void UpdateViewFilters()
        {
            FilterRefreshNeeded = false;
            foreach (EditorEntity entity in entities)
			{
				entity.SetFilter();
			}
        }
        internal void Flip(FlipDirection direction)
        {
            var positions = selectedEntities.Select(se => se.Entity.Position);
            IEnumerable<Position.Value> monoCoordinatePositions;
            if (direction == FlipDirection.Horizontal)
            {
                monoCoordinatePositions = positions.Select(p => p.X);
            }
            else
            {
                monoCoordinatePositions = positions.Select(p => p.Y);
            }

            short min = monoCoordinatePositions.Min(m => m.High);
            short max = monoCoordinatePositions.Max(m => m.High);
            int diff = max - min;

            foreach (var entity in selectedEntities)
            {
                if (direction == FlipDirection.Horizontal)
                {
                    short xPos = entity.Entity.Position.X.High;
                    int fromLeft = xPos - min;
                    int fromRight = max - xPos;

                    int newX = fromLeft < fromRight ? max - fromLeft : min + fromRight;
                    entity.Entity.Position.X.High = (short)newX;
                }
                else
                {
                    short yPos = entity.Entity.Position.Y.High;
                    int fromBottom = yPos - min;
                    int fromTop = max - yPos;

                    int newY = fromBottom < fromTop ? max - fromBottom : min + fromTop;
                    entity.Entity.Position.Y.High = (short)newY;
                }

                entity.Flip(direction);
            }
        }
    }
}
