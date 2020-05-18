using System;
using System.Collections.Generic;
using System.Text;

namespace GongSolutions.Wpf.DragDrop
{
    public class VisualTargetInfo  // Smallpdf
    {
        // if VisualTargetOrientation is horizontal, the indicator will be vertical
        public bool ForceVisualTargetOrientationHorizontal { get; set; }

        public bool UseImprovedVisualTargetInsertLine { get; set; }

        // These 4 line settings are only applyed if UseImprovedVisualTargetInsertLine is true
        public int VerticalInsertLineTop { get; set; }  // Verified orig. design Zeplin
        public int VerticalInsertLineHeight { get; set; }  // Verified orig. design Zeplin

        public int HorizontalInsertLineLeft { get; set; }
        public int HorizontalInsertLineWidth { get; set; }
    }
}
