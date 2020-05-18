using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;

namespace GongSolutions.Wpf.DragDrop.SerializableHandlers
{
    public delegate void OnDragDelegate(IDragInfo dragInfo);   // New Smallpdf
    public delegate void NoParamsDelegate();   // New Smallpdf

    [Serializable]
    public class SerializableWrapper
    {
        public IEnumerable<object> Items { get; set; }

        public DragDropKeyStates DragDropCopyKeyState { get; set; }
    }

    public class SerializableDragHandler : GongSolutions.Wpf.DragDrop.IDragSource
    {
        public OnDragDelegate OnStartDrag = null;   // New Smallpdf
        public NoParamsDelegate OnDragCancelled = null;   // New Smallpdf

        private bool alreadyDropped = false;

        public void StartDrag(IDragInfo dragInfo)
        {
            alreadyDropped = false;
            var items = dragInfo.SourceItems.OfType<object>().ToList();
            var wrapper = new SerializableWrapper()
            {
                Items = items,
                DragDropCopyKeyState = DragDropKeyStates.ControlKey //dragInfo.DragDropCopyKeyState
            };
            dragInfo.Data = wrapper;
            dragInfo.DataFormat = DataFormats.GetDataFormat(DataFormats.Serializable);
            dragInfo.Effects = dragInfo.Data != null ? DragDropEffects.Copy | DragDropEffects.Move : DragDropEffects.None;

            if (OnStartDrag != null)   // New Smallpdf
                OnStartDrag(dragInfo);
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return true;
        }

        public void Dropped(IDropInfo dropInfo)
        {
            alreadyDropped = true;
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
        {
            if (alreadyDropped || dragInfo == null)
            {
                return;
            }

            // the drag operation has finished on another app
            if (operationResult != DragDropEffects.None)
            {
                if (operationResult.HasFlag(DragDropEffects.Move))
                {
                    var sourceList = dragInfo.SourceCollection.TryGetList();
                    var items = dragInfo.SourceItems.OfType<object>().ToList();
                    if (sourceList != null)
                    {
                        foreach (var o in items)
                        {
                            sourceList.Remove(o);
                        }
                    }
                    alreadyDropped = true;
                }
            }
        }

        public void DragCancelled()
        {
            if (OnDragCancelled != null)   // New Smallpdf
                OnDragCancelled();
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return false;
        }

        public static SerializableWrapper GetSerializableWrapper(IDragInfo dropInfo)   // Smallpdf
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
    }

}
