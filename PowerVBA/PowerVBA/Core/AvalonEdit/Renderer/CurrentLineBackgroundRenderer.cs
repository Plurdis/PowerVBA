﻿using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PowerVBA.Core.AvalonEdit.Renderer
{
    public class CurrentLineBackgroundRenderer : IBackgroundRenderer
    {
        private TextEditor _editor;

        public CurrentLineBackgroundRenderer(TextEditor editor)
        {
            _editor = editor;
        }

        public KnownLayer Layer
        {
            get { return KnownLayer.Caret; }
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_editor.Document == null)
                return;

            textView.EnsureVisualLines();
            var currentLine = _editor.Document.GetLineByOffset(_editor.CaretOffset);
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
            {
                try
                {
                    drawingContext.DrawRectangle(
                    null, new Pen(Brushes.Gray,0.5),
                    new Rect(new Point(rect.Location.X + 1, rect.Location.Y), new Size(textView.ActualWidth - 2, rect.Height)));
                }
                catch (Exception)
                { }
                
            }
        }
    }
}