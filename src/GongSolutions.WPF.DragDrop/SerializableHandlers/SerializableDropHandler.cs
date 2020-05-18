using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace GongSolutions.Wpf.DragDrop.SerializableHandlers
{
    public delegate void OnDropDelegate(int originIndex, int destIndex, IEnumerable<object> items);   // New Smallpdf
    public delegate void DragOverDelegate(IDropInfo dropInfo);

    public class SerializableDropHandler : IDropTarget
    {
        public OnDropDelegate OnDropAndMove = null;   // New Smallpdf
        public DragOverDelegate OnDragOver = null;   // New Smallpdf

        private int lastInsertIndexDragOver;


        public void DragOver(IDropInfo dropInfo)
        {
            Debug.WriteLine("At top of SerializableDropHandler.DragOver");

            if (dropInfo.GoingToDropNow)   // New Smallpdf
                return;

            var wrapper = GetSerializableWrapper(dropInfo);
            if (wrapper != null && dropInfo.TargetCollection != null)
            {
                dropInfo.Effects = ShouldCopyData(dropInfo, wrapper.DragDropCopyKeyState) ? DragDropEffects.Copy : DragDropEffects.Move;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }

            lastInsertIndexDragOver = dropInfo.InsertIndex;
            Debug.WriteLine("Inside DragOver. InsertIndex=" + dropInfo.InsertIndex);
            if (OnDragOver != null)   // New Smallpdf
                OnDragOver(dropInfo);
        }

        public void Drop(IDropInfo dropInfo)
        {
            var wrapper = GetSerializableWrapper(dropInfo);
            if (wrapper != null && dropInfo.TargetCollection != null)
            {
                // at this point the drag info can be null, cause the other app doesn't know it
                var insertIndex = dropInfo.InsertIndex != dropInfo.UnfilteredInsertIndex ? dropInfo.UnfilteredInsertIndex : dropInfo.InsertIndex;
                var destinationList = dropInfo.TargetCollection.TryGetList();
                var copyData = ShouldCopyData(dropInfo, wrapper.DragDropCopyKeyState);

                insertIndex = lastInsertIndexDragOver;   // This seems odd, but for some reason the insertIndex calculated 3 lines above wasn't always right

                var firstOriIndex = -1;
                var firstDestIndex = insertIndex;

                if (!copyData)
                {
                    var sourceList = dropInfo.DragInfo?.SourceCollection?.TryGetList();
                    if (sourceList != null)
                    {
                        if (wrapper.Items.First() != null)
                            firstOriIndex = sourceList.IndexOf(wrapper.Items.First());
                        foreach (var o in wrapper.Items)
                        {
                            var index = sourceList.IndexOf(o);
                            if (index != -1)
                            {
                                sourceList.RemoveAt(index);
                                // so, is the source list the destination list too ?
                                if (destinationList != null && Equals(sourceList, destinationList) && index < insertIndex)
                                {
                                    --insertIndex;
                                }
                            }
                        }
                    }
                }

                if (destinationList != null)
                {
                    // check for cloning
                    var cloneData = dropInfo.Effects.HasFlag(DragDropEffects.Copy)
                                    || dropInfo.Effects.HasFlag(DragDropEffects.Link);
                    foreach (var o in wrapper.Items)
                    {
                        var obj2Insert = o;
                        if (cloneData)
                        {
                            var cloneable = o as ICloneable;
                            if (cloneable != null)
                            {
                                obj2Insert = cloneable.Clone();
                            }
                        }

                        destinationList.Insert(insertIndex++, obj2Insert);
                    }
                }

                if (OnDropAndMove != null)   // New Smallpdf
                    OnDropAndMove(firstOriIndex, firstDestIndex, wrapper.Items);

            }
        }

        public static SerializableWrapper GetSerializableWrapper(IDropInfo dropInfo)
        {
            var data = dropInfo.Data;

            var dataObject = data as DataObject;
            if (dataObject != null)
            {
                var dataFormat = DataFormats.GetDataFormat(DataFormats.Serializable);
                data = dataObject.GetDataPresent(dataFormat.Name) ? dataObject.GetData(dataFormat.Name) : data;
            }

            var wrapper = data as SerializableWrapper;
            return wrapper;
        }

        private static bool ShouldCopyData(IDropInfo dropInfo, DragDropKeyStates dragDropCopyKeyState)
        {
            // default should always the move action/effect
            if (dropInfo == null)
            {
                return false;
            }
            var copyData = ((dragDropCopyKeyState != default(DragDropKeyStates)) && dropInfo.KeyStates.HasFlag(dragDropCopyKeyState))
                           || dragDropCopyKeyState.HasFlag(DragDropKeyStates.LeftMouseButton);
            return copyData;
        }
    }
}
