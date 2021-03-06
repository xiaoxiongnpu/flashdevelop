using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using PluginCore.Helpers;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class VS2005AutoHideStrip : AutoHideStripBase
    {
        class TabVS2005 : Tab
        {
            internal TabVS2005(IDockContent content)
                : base(content)
            {
            }

            int m_tabX = 0;
            public int TabX
            {
                get => m_tabX;
                set => m_tabX = value;
            }

            int m_tabWidth = 0;
            public int TabWidth
            {
                get => m_tabWidth;
                set => m_tabWidth = value;
            }

        }

        // CHANGED - NICK
        const int _ImageHeight = 16;
        const int _ImageWidth = 16;
        const int _ImageGapTop = 4;
        const int _ImageGapLeft = 5;
        const int _ImageGapRight = 2;
        const int _ImageGapBottom = 2;
        const int _TextGapLeft = 0;
        const int _TextGapRight = 3;
        const int _TabGapTop = 3;
        const int _TabGapLeft = 3;
        const int _TabGapBetween = 4;

        #region Customizable Properties

        static StringFormat _stringFormatTabHorizontal;

        StringFormat StringFormatTabHorizontal
        {
            get
            {
                if (_stringFormatTabHorizontal is null)
                {
                    _stringFormatTabHorizontal = new StringFormat();
                    _stringFormatTabHorizontal.Alignment = StringAlignment.Near;
                    _stringFormatTabHorizontal.LineAlignment = StringAlignment.Center;
                    _stringFormatTabHorizontal.FormatFlags = StringFormatFlags.NoWrap;
                }

                if (RightToLeft == RightToLeft.Yes)
                    _stringFormatTabHorizontal.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                else
                    _stringFormatTabHorizontal.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;

                return _stringFormatTabHorizontal;
            }
        }

        static StringFormat _stringFormatTabVertical;

        StringFormat StringFormatTabVertical
        {
            get
            {   
                if (_stringFormatTabVertical is null)
                {
                    _stringFormatTabVertical = new StringFormat();
                    _stringFormatTabVertical.Alignment = StringAlignment.Near;
                    _stringFormatTabVertical.LineAlignment = StringAlignment.Center;
                    _stringFormatTabVertical.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.DirectionVertical;
                }
                if (RightToLeft == RightToLeft.Yes)
                    _stringFormatTabVertical.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                else
                    _stringFormatTabVertical.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;

                return _stringFormatTabVertical;
            }
        }

        static int ImageHeight => ScaleHelper.Scale(_ImageHeight);

        static int ImageWidth => ScaleHelper.Scale(_ImageWidth);

        static int ImageGapTop => ScaleHelper.Scale(_ImageGapTop);

        static int ImageGapLeft => ScaleHelper.Scale(_ImageGapLeft);

        static int ImageGapRight => ScaleHelper.Scale(_ImageGapRight);

        static int ImageGapBottom => ScaleHelper.Scale(_ImageGapBottom);

        static int TextGapLeft => ScaleHelper.Scale(_TextGapLeft);

        static int TextGapRight => ScaleHelper.Scale(_TextGapRight);

        static int TabGapTop => ScaleHelper.Scale(_TabGapTop);

        static int TabGapLeft => ScaleHelper.Scale(_TabGapLeft);

        static int TabGapBetween => ScaleHelper.Scale(_TabGapBetween);

        static Brush BrushTabBackground
        {
            get 
            {
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005AutoHideStrip.BackColor");
                if (color != Color.Empty) return new SolidBrush(color);
                return SystemBrushes.Control;   
            }
        }

        static Pen PenTabBorder
        {
            get 
            {
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005AutoHideStrip.BorderColor");
                if (color != Color.Empty) return new Pen(color);
                return SystemPens.ControlDark;
            }
        }

        static Brush BrushTabText
        {
            get 
            {
                Color color = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005AutoHideStrip.ForeColor");
                if (color != Color.Empty) return new SolidBrush(color);
                return SystemBrushes.FromSystemColor(SystemColors.ControlDarkDark);
            }
        }
        #endregion

        static readonly Matrix _matrixIdentity = new Matrix();
        static Matrix MatrixIdentity => _matrixIdentity;

        static DockState[] _dockStates;

        static DockState[] DockStates
        {
            get
            {
                if (_dockStates is null)
                {
                    _dockStates = new DockState[4];
                    _dockStates[0] = DockState.DockLeftAutoHide;
                    _dockStates[1] = DockState.DockRightAutoHide;
                    _dockStates[2] = DockState.DockTopAutoHide;
                    _dockStates[3] = DockState.DockBottomAutoHide;
                }
                return _dockStates;
            }
        }

        static GraphicsPath _graphicsPath;
        internal static GraphicsPath GraphicsPath
        {
            get
            {
                if (_graphicsPath is null)
                    _graphicsPath = new GraphicsPath();

                return _graphicsPath;
            }
        }

        public VS2005AutoHideStrip(DockPanel panel) : base(panel)
        {
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            BackColor = SystemColors.Control;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            DrawTabStrip(g);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            CalculateTabs();
            base.OnLayout (levent);
        }

        void DrawTabStrip(Graphics g)
        {
            DrawTabStrip(g, DockState.DockTopAutoHide);
            DrawTabStrip(g, DockState.DockBottomAutoHide);
            DrawTabStrip(g, DockState.DockLeftAutoHide);
            DrawTabStrip(g, DockState.DockRightAutoHide);
        }

        void DrawTabStrip(Graphics g, DockState dockState)
        {
            Rectangle rectTabStrip = GetLogicalTabStripRectangle(dockState);

            if (rectTabStrip.IsEmpty)
                return;

            Matrix matrixIdentity = g.Transform;
            if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
            {
                Matrix matrixRotated = new Matrix();
                matrixRotated.RotateAt(90, new PointF(rectTabStrip.X + (float)rectTabStrip.Height / 2,
                    rectTabStrip.Y + (float)rectTabStrip.Height / 2));
                g.Transform = matrixRotated;
            }

            foreach (Pane pane in GetPanes(dockState))
            {
                foreach (TabVS2005 tab in pane.AutoHideTabs)
                    DrawTab(g, tab);
            }
            g.Transform = matrixIdentity;
        }

        void CalculateTabs()
        {
            CalculateTabs(DockState.DockTopAutoHide);
            CalculateTabs(DockState.DockBottomAutoHide);
            CalculateTabs(DockState.DockLeftAutoHide);
            CalculateTabs(DockState.DockRightAutoHide);
        }

        void CalculateTabs(DockState dockState)
        {
            Rectangle rectTabStrip = GetLogicalTabStripRectangle(dockState);

            int imageHeight = rectTabStrip.Height - ImageGapTop - ImageGapBottom;
            int imageWidth = ImageWidth;
            if (imageHeight > ImageHeight)
                imageWidth = ImageWidth * (imageHeight / ImageHeight);

            // HACK - Mika
            int x;
            if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
            {
                x = rectTabStrip.X;
            }
            else x = TabGapLeft + rectTabStrip.X;

            string tabStyle = PluginCore.PluginBase.MainForm.GetThemeValue("VS2005AutoHideStrip.TabStyle");

            foreach (Pane pane in GetPanes(dockState))
            {
                foreach (TabVS2005 tab in pane.AutoHideTabs)
                {
                    int width;

                    if (tabStyle == "Underlined") width = TextRenderer.MeasureText(tab.Content.DockHandler.TabText, Font).Width + TextGapLeft + TextGapRight;
                    else width = imageWidth + ImageGapLeft + ImageGapRight + TextRenderer.MeasureText(tab.Content.DockHandler.TabText, Font).Width + TextGapLeft + TextGapRight;
                    
                    tab.TabX = x;
                    tab.TabWidth = width;
                    x += width;
                }

                x += TabGapBetween ;
            }
        }

        Rectangle RtlTransform(Rectangle rect, DockState dockState)
        {
            Rectangle rectTransformed;
            if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
                rectTransformed = rect;
            else
                rectTransformed = DrawHelper.RtlTransform(this, rect);

            return rectTransformed;
        }

        GraphicsPath GetTabOutline(TabVS2005 tab, bool transformed, bool rtlTransform)
        {
            DockState dockState = tab.Content.DockHandler.DockState;
            Rectangle rectTab = GetTabRectangle(tab, transformed);
            if (rtlTransform)
                rectTab = RtlTransform(rectTab, dockState);
            bool upTab = (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockBottomAutoHide);
            DrawHelper.GetRoundedCornerTab(GraphicsPath, rectTab, upTab);

            return GraphicsPath;
        }

        void DrawTab(Graphics g, TabVS2005 tab)
        {
            Rectangle rectTabOrigin = GetTabRectangle(tab);
            if (rectTabOrigin.IsEmpty)
                return;

            DockState dockState = tab.Content.DockHandler.DockState;
            IDockContent content = tab.Content;

            GraphicsPath path = GetTabOutline(tab, false, true);
            g.FillPath(BrushTabBackground, path);

            string tabStyle = PluginCore.PluginBase.MainForm.GetThemeValue("VS2005AutoHideStrip.TabStyle");
            Color tabUlColor = PluginCore.PluginBase.MainForm.GetThemeColor("VS2005AutoHideStrip.TabUnderlineColor");

            if (tabStyle == "Underlined")
            {
                int spacing = ScaleHelper.Scale(4);
                Brush brush = tabUlColor != Color.Empty ? new SolidBrush(tabUlColor) : SystemBrushes.Highlight;
                if (dockState == DockState.DockRightAutoHide)
                {
                    g.FillRectangle(brush, new Rectangle(rectTabOrigin.Left + spacing, rectTabOrigin.Y, rectTabOrigin.Width - (spacing * 2), spacing));
                    rectTabOrigin.Y += spacing;
                }
                else if (dockState == DockState.DockTopAutoHide)
                {
                    g.FillRectangle(brush, new Rectangle(rectTabOrigin.X + spacing, rectTabOrigin.Bottom - (spacing / 3), rectTabOrigin.Width - (spacing * 2), rectTabOrigin.Bottom));
                    rectTabOrigin.Y -= (spacing / 3);
                }
                else
                {
                    g.FillRectangle(brush, new Rectangle(rectTabOrigin.X + spacing, rectTabOrigin.Bottom - spacing, rectTabOrigin.Width - (spacing * 2), rectTabOrigin.Bottom));
                    rectTabOrigin.Y -= spacing;
                }
            }
            else g.DrawPath(PenTabBorder, path);

            // Set no rotate for drawing icon and text
            Matrix matrixRotate = g.Transform;
            g.Transform = MatrixIdentity;

            // Draw the icon
            Rectangle rectImage = rectTabOrigin;

            // HACK - This makes the Silk icon set look better (although it is NOT VS 2005 behavior)
            if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
                rectImage.Y -= 1;

            rectImage.X += ImageGapLeft;
            rectImage.Y += ImageGapTop;
            int imageHeight = rectTabOrigin.Height - ImageGapTop - ImageGapBottom;
            int imageWidth = ImageWidth;
            if (imageHeight > ImageHeight)
                imageWidth = ImageWidth * (imageHeight/ImageHeight);
            rectImage.Height = imageHeight;
            rectImage.Width = imageWidth;
            rectImage = GetTransformedRectangle(dockState, rectImage);

            if (tabStyle != "Underlined") g.DrawIcon(((Form)content).Icon, RtlTransform(rectImage, dockState));

            // Draw the text
            Rectangle rectText = rectTabOrigin;

            // CHANGED - Mika
            if (Font.SizeInPoints > 8F) rectText.Y += 1;

            if (tabStyle == "Underlined") rectText.X += TextGapRight;
            else
            {
                rectText.X += ImageGapLeft + imageWidth + ImageGapRight + TextGapLeft;
                rectText.Width -= ImageGapLeft + imageWidth + ImageGapRight + TextGapLeft;
            }

            rectText = RtlTransform(GetTransformedRectangle(dockState, rectText), dockState);
            if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
                g.DrawString(content.DockHandler.TabText, Font, BrushTabText, rectText, StringFormatTabVertical);
            else
                g.DrawString(content.DockHandler.TabText, Font, BrushTabText, rectText, StringFormatTabHorizontal);

            // Set rotate back
            g.Transform = matrixRotate;
        }

        Rectangle GetLogicalTabStripRectangle(DockState dockState)
        {
            return GetLogicalTabStripRectangle(dockState, false);
        }

        Rectangle GetLogicalTabStripRectangle(DockState dockState, bool transformed)
        {
            if (!DockHelper.IsDockStateAutoHide(dockState))
                return Rectangle.Empty;

            int leftPanes = GetPanes(DockState.DockLeftAutoHide).Count;
            int rightPanes = GetPanes(DockState.DockRightAutoHide).Count;
            int topPanes = GetPanes(DockState.DockTopAutoHide).Count;
            int bottomPanes = GetPanes(DockState.DockBottomAutoHide).Count;

            int x, y, width, height;

            height = MeasureHeight();
            if (dockState == DockState.DockLeftAutoHide && leftPanes > 0)
            {
                x = 0;
                y = (topPanes == 0) ? 0 : height;
                width = Height - (topPanes == 0 ? 0 : height) - (bottomPanes == 0 ? 0 :height);
            }
            else if (dockState == DockState.DockRightAutoHide && rightPanes > 0)
            {
                x = Width - height;
                if (leftPanes != 0 && x < height)
                    x = height;
                y = (topPanes == 0) ? 0 : height;
                width = Height - (topPanes == 0 ? 0 : height) - (bottomPanes == 0 ? 0 :height);
            }
            else if (dockState == DockState.DockTopAutoHide && topPanes > 0)
            {
                x = leftPanes == 0 ? 0 : height;
                y = 0;
                width = Width - (leftPanes == 0 ? 0 : height) - (rightPanes == 0 ? 0 : height);
            }
            else if (dockState == DockState.DockBottomAutoHide && bottomPanes > 0)
            {
                x = leftPanes == 0 ? 0 : height;
                y = Height - height;
                if (topPanes != 0 && y < height)
                    y = height;
                width = Width - (leftPanes == 0 ? 0 : height) - (rightPanes == 0 ? 0 : height);
            }
            else
                return Rectangle.Empty;

            if (!transformed)
                return new Rectangle(x, y, width, height);
            return GetTransformedRectangle(dockState, new Rectangle(x, y, width, height));
        }

        Rectangle GetTabRectangle(TabVS2005 tab)
        {
            return GetTabRectangle(tab, false);
        }

        Rectangle GetTabRectangle(TabVS2005 tab, bool transformed)
        {
            DockState dockState = tab.Content.DockHandler.DockState;
            Rectangle rectTabStrip = GetLogicalTabStripRectangle(dockState);

            if (rectTabStrip.IsEmpty)
                return Rectangle.Empty;

            int x = tab.TabX;
            int y = rectTabStrip.Y + 
                (dockState == DockState.DockTopAutoHide || dockState == DockState.DockRightAutoHide ?
                0 : TabGapTop);
            int width = tab.TabWidth;
            int height = rectTabStrip.Height - TabGapTop;

            if (!transformed)
                return new Rectangle(x, y, width, height);
            return GetTransformedRectangle(dockState, new Rectangle(x, y, width, height));
        }

        Rectangle GetTransformedRectangle(DockState dockState, Rectangle rect)
        {
            if (dockState != DockState.DockLeftAutoHide && dockState != DockState.DockRightAutoHide)
                return rect;

            PointF[] pts = new PointF[1];
            // the center of the rectangle
            pts[0].X = rect.X + (float)rect.Width / 2;
            pts[0].Y = rect.Y + (float)rect.Height / 2;
            Rectangle rectTabStrip = GetLogicalTabStripRectangle(dockState);
            Matrix matrix = new Matrix();
            matrix.RotateAt(90, new PointF(rectTabStrip.X + (float)rectTabStrip.Height / 2,
                rectTabStrip.Y + (float)rectTabStrip.Height / 2));
            matrix.TransformPoints(pts);

            return new Rectangle((int)(pts[0].X - (float)rect.Height / 2 + .5F),
                (int)(pts[0].Y - (float)rect.Width / 2 + .5F),
                rect.Height, rect.Width);
        }

        protected override IDockContent HitTest(Point ptMouse)
        {
            foreach(DockState state in DockStates)
            {
                Rectangle rectTabStrip = GetLogicalTabStripRectangle(state, true);
                if (!rectTabStrip.Contains(ptMouse))
                    continue;

                foreach(Pane pane in GetPanes(state))
                {
                    foreach(TabVS2005 tab in pane.AutoHideTabs)
                    {
                        GraphicsPath path = GetTabOutline(tab, true, true);
                        if (path.IsVisible(ptMouse))
                            return tab.Content;
                    }
                }
            }
            
            return null;
        }

        protected internal override int MeasureHeight()
        {
            return Math.Max(ImageGapBottom +
                ImageGapTop + ImageHeight,
                Font.Height) + TabGapTop;
        }

        protected override void OnRefreshChanges()
        {
            CalculateTabs();
            Invalidate();
        }

        protected override AutoHideStripBase.Tab CreateTab(IDockContent content)
        {
            return new TabVS2005(content);
        }
    }
}
