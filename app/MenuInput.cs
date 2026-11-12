using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Judge
{
    internal enum PointerResult{None,Click,Drag}
    internal sealed class PointerGesture
    {
        public bool Pressed{get;private set;}public bool Dragging{get;private set;}public Point Origin{get;private set;}MouseButtons button;
        public void Begin(MouseButtons value,Point point){button=value;Origin=point;Pressed=true;Dragging=false;}
        public void Move(Point point){if(Pressed&&button==MouseButtons.Left&&Math.Abs(point.X-Origin.X)+Math.Abs(point.Y-Origin.Y)>=6)Dragging=true;}
        public PointerResult End(MouseButtons value)
        {if(!Pressed||button!=value)return PointerResult.None;PointerResult result=Dragging?PointerResult.Drag:PointerResult.Click;Pressed=false;Dragging=false;return result;}
        public void Cancel(){Pressed=false;Dragging=false;}
    }
    internal static class MenuInput
    {
        public static bool Inside(Point point,IEnumerable<Rectangle> openMenus)
        {foreach(Rectangle bounds in openMenus)if(bounds.Contains(point))return true;return false;}
        public static bool Dismiss(Point point,int buttons,int previousButtons,bool escape,IEnumerable<Rectangle> openMenus)
        {return escape||((buttons&~previousButtons)!=0&&!Inside(point,openMenus));}
    }
}
