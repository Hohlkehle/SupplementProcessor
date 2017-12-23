using SupplementProcessor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ControlManager
{
    internal class ControlMoverOrResizer
    {
        private static bool _moving;
        private static Point _cursorStartPoint;
        private static bool _moveIsInterNal;
        private static bool _resizing;
        private static Size _currentControlStartSize;
        internal static bool MouseIsInLeftEdge { get; set; }
        internal static bool MouseIsInRightEdge { get; set; }
        internal static bool MouseIsInTopEdge { get; set; }
        internal static bool MouseIsInBottomEdge { get; set; }

        internal enum MoveOrResize
        {
            Move,
            Resize,
            MoveAndResize
        }

        public Point MousePosition
        {
            set
            {
                System.Windows.Forms.Cursor.Position = new Point(value.X, value.Y);
            }
            get { return new Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y); }
        }

        internal static MoveOrResize WorkType { get; set; }

        internal static void Init(Control control)
        {
            Init(control, control);
        }

        internal static void Init(Control control, Control container)
        {
            _moving = false;
            _resizing = false;
            _moveIsInterNal = false;
            _cursorStartPoint = new Point();
            MouseIsInLeftEdge = false;
            MouseIsInLeftEdge = false;
            MouseIsInRightEdge = false;
            MouseIsInTopEdge = false;
            MouseIsInBottomEdge = false;
            WorkType = MoveOrResize.MoveAndResize;
            control.MouseDown += (sender, e) => StartMovingOrResizing(control, e);
            control.MouseUp += (sender, e) => StopDragOrResizing(control);
            control.MouseMove += (sender, e) => MoveControl(container, e);
        }

        private static void UpdateMouseEdgeProperties(Control control, Point mouseLocationInControl)
        {
            if (WorkType == MoveOrResize.Move)
            {
                return;
            }
            MouseIsInLeftEdge = Math.Abs(mouseLocationInControl.X) <= 2;
            MouseIsInRightEdge = Math.Abs(mouseLocationInControl.X - control.Width) <= 2;
            MouseIsInTopEdge = Math.Abs(mouseLocationInControl.Y ) <= 2;
            MouseIsInBottomEdge = Math.Abs(mouseLocationInControl.Y - control.Height) <= 2;
        }

        private static void UpdateMouseCursor(Control control)
        {
            if (WorkType == MoveOrResize.Move)
            {
                return;
            }
            if (MouseIsInLeftEdge )
            {
                if (MouseIsInTopEdge)
                {
                    control.Cursor = Cursors.SizeNWSE;
                }
                else if (MouseIsInBottomEdge)
                {
                    control.Cursor = Cursors.SizeNESW;
                }
                else
                {
                    control.Cursor = Cursors.SizeWE;
                }
            }
            else if (MouseIsInRightEdge)
            {
                if (MouseIsInTopEdge)
                {
                    control.Cursor = Cursors.SizeNESW;
                }
                else if (MouseIsInBottomEdge)
                {
                    control.Cursor = Cursors.SizeNWSE;
                }
                else
                {
                    control.Cursor = Cursors.SizeWE;
                }
            }
            else if (MouseIsInTopEdge || MouseIsInBottomEdge)
            {
                control.Cursor = Cursors.SizeNS;
            }
            else
            {
                control.Cursor = Cursors.Arrow;
            }
        }

        private static void StartMovingOrResizing(Control control, MouseEventArgs e)
        {
            if (_moving || _resizing)
            {
                return;
            }
            if (WorkType!=MoveOrResize.Move &&
                (MouseIsInRightEdge || MouseIsInLeftEdge || MouseIsInTopEdge || MouseIsInBottomEdge))
            {
                _resizing = true;
                _currentControlStartSize = control.Size;
            }
            else if (WorkType!=MoveOrResize.Resize)
            {
                _moving = true;
                control.Cursor = Cursors.Hand;
            }

            var pos = e.GetPosition((UIElement)MainWindow.instance);

            _cursorStartPoint = new Point(pos.X, pos.Y);

            Mouse.Capture(control);
            //control.Capture = true;
        }

        private static void MoveControl(Control control, MouseEventArgs e)
        {
            var pos = e.GetPosition((UIElement)MainWindow.instance);

            if (!_resizing && ! _moving)
            {
                UpdateMouseEdgeProperties(control, new Point(pos.X, pos.Y));
                UpdateMouseCursor(control);
            }
            if (_resizing)
            {
                if (MouseIsInLeftEdge)
                {
                    if (MouseIsInTopEdge)
                    {

                        //control.TranslatePoint(new Point(0, 0), this).X


                        var initialLocation = control.TranslatePoint(new Point(0, 0), MainWindow.instance);
                        control.RenderTransform = new TranslateTransform(element.Left - initialLocation.X, element.Top - initialLocation.Y);
               
                        control.Width -= (pos.X - _cursorStartPoint.X);
                        //control.Left += (pos.X - _cursorStartPoint.X); 
                        control.Height -= (pos.Y - _cursorStartPoint.Y);
                        //control.Top += (pos.Y - _cursorStartPoint.Y);
                    }
                    else if (MouseIsInBottomEdge)
                    {
                        control.Width -= (pos.X - _cursorStartPoint.X);
                        control.Left += (pos.X - _cursorStartPoint.X);
                        control.Height = (pos.Y - _cursorStartPoint.Y) + _currentControlStartSize.Height;                    
                    }
                    else
                    {
                        control.Width -= (pos.X - _cursorStartPoint.X);
                        control.Left += (pos.X - _cursorStartPoint.X) ;
                    }
                }
                else if (MouseIsInRightEdge)
                {
                    if (MouseIsInTopEdge)
                    {
                        control.Width = (pos.X - _cursorStartPoint.X) + _currentControlStartSize.Width;
                        control.Height -= (pos.Y - _cursorStartPoint.Y);
                        control.Top += (pos.Y - _cursorStartPoint.Y);

                    }
                    else if (MouseIsInBottomEdge)
                    {
                        control.Width = (pos.X - _cursorStartPoint.X) + _currentControlStartSize.Width;
                        control.Height = (pos.Y - _cursorStartPoint.Y) + _currentControlStartSize.Height;                    
                    }
                    else
                    {
                        control.Width = (pos.X - _cursorStartPoint.X)+_currentControlStartSize.Width;
                    }
                }
                else if (MouseIsInTopEdge)
                {
                    control.Height -= (pos.Y - _cursorStartPoint.Y);
                    control.Top += (pos.Y - _cursorStartPoint.Y);
                }
                else if (MouseIsInBottomEdge)
                {
                    control.Height = (pos.Y - _cursorStartPoint.Y) + _currentControlStartSize.Height;                    
                }
                else
                {
                     StopDragOrResizing(control);
                }
            }
            else if (_moving)
            {
                _moveIsInterNal = !_moveIsInterNal;
                if (!_moveIsInterNal)
                {
                    int x = (pos.X - _cursorStartPoint.X) + control.Left;
                    int y = (pos.Y - _cursorStartPoint.Y) + control.Top;
                    control.Location = new Point(x, y);
                }
            }
        }

        private static void StopDragOrResizing(Control control)
        {
            _resizing = false;
            _moving = false;
            control.Capture = false;
            UpdateMouseCursor(control);
        }

        #region Save And Load

        private static List<Control> GetAllChildControls(Control control, List<Control> list)
        {
            List<Control> controls = control.Controls.Cast<Control>().ToList();
            list.AddRange(controls);
            return controls.SelectMany(ctrl => GetAllChildControls(ctrl, list)).ToList();
        }

        internal static string GetSizeAndPositionOfControlsToString(Control container)
        {
            List<Control> controls = new List<Control>();
            GetAllChildControls(container, controls);
            CultureInfo cultureInfo = new CultureInfo("en");
            string info = string.Empty;
            foreach (Control control in controls)
            {
                info += control.Name + ":" + control.Left.ToString(cultureInfo) + "," + control.Top.ToString(cultureInfo) + "," +
                        control.Width.ToString(cultureInfo) + "," + control.Height.ToString(cultureInfo) + "*";
            }
            return info;
        }
        internal static void SetSizeAndPositionOfControlsFromString(Control container, string controlsInfoStr)
        {
            List<Control> controls = new List<Control>();
            GetAllChildControls(container, controls);
            string[] controlsInfo = controlsInfoStr.Split(new []{"*"},StringSplitOptions.RemoveEmptyEntries );
            Dictionary<string, string> controlsInfoDictionary = new Dictionary<string, string>();
            foreach (string controlInfo in controlsInfo)
            {
                string[] info = controlInfo.Split(new [] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                controlsInfoDictionary.Add(info[0], info[1]);
            }
            foreach (Control control in controls)
            {
                string propertiesStr;
                controlsInfoDictionary.TryGetValue(control.Name, out propertiesStr);
                string[] properties = propertiesStr.Split(new [] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (properties.Length == 4)
                {
                    control.Left = int.Parse(properties[0]);
                    control.Top = int.Parse(properties[1]);
                    control.Width = int.Parse(properties[2]);
                    control.Height = int.Parse(properties[3]);
                }
            }
        }

        #endregion
    }
}