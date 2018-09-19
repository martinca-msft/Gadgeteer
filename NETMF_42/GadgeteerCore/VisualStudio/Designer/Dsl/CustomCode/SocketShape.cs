// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.Modeling.Diagrams;
using EnvDTE;
using System.Runtime.InteropServices;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Customization of the socket shapes (Socket and SocketUse)
    /// </summary>
	partial class SocketShapeBase
	{
        private const float PenWidth = 0.02f;
		private bool highlight;

        private static readonly Pen UnconnectedSocketPen = new Pen(Color.FromArgb(255, 255, 180, 0), PenWidth);
        private static readonly Pen CanConnectSocketFeedbackPen = new Pen(Color.FromArgb(255, 113, 195, 27), PenWidth);
        private static readonly Pen ConnectedSocketPen = new Pen(Color.FromArgb(255, 0, 186, 255), PenWidth);

        private static readonly SolidBrush CanConnectSocketFeedbackFill = new SolidBrush(Color.FromArgb(178, 90, 154, 21));
        private static readonly SolidBrush ConnectedSocketFill = new SolidBrush(Color.FromArgb(153, 3, 125, 170));


        //Socket size, in inches
        private const float rectW = (4.5f / 25.4f);
        private const float rectH = (12f / 25.4f);

		// Handling the delete press here instead of an accelerator key because this is specific to the socket shape where
		// an accelerator key is global to the design window and we would have to handle every senario (text box editing, module deleting)
		public override void OnKeyUp(DiagramKeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (e.KeyCode == System.Windows.Forms.Keys.Delete)
			{
				var dte = this.Diagram.GetService(typeof(DTE)) as DTE;
                if (dte != null)
                {
                    try
                    {                        
                        dte.ExecuteCommand("OtherContextMenus.GadgeteerDSLContext.Disconnect");
                    }
                    catch (COMException)
                    {
                        //This can happen for sockets when they are not connected
                    }
                }
			}
		}

	
		//Do our own painting to support arbitrary socket orientations and different socket states (normal, highlighted, connected)
		public override void OnPaintShape(DiagramPaintEventArgs e)
		{
			var socket = this.ModelElement as SocketBase;
			if (socket == null || socket.Definition == null)
			{
				Log.WriteWarning("Socket or socket definition not found for socket shape");
				return;
			}

			var g = e.Graphics;

			//Important to save and restore because otherwise we disturb the dc for all other shapes
			var state = g.Save();

			g.TranslateTransform((float)(AbsoluteBoundingBox.Left + Size.Width / 2), (float)(AbsoluteBoundingBox.Top + Size.Height / 2));
			g.RotateTransform((float)socket.Definition.Orientation*-1);

			var points = new PointF[] {
				new PointF(rectW/2, -rectH/8),
				new PointF(rectW/2,-rectH/2),
				new PointF(-rectW/2,-rectH/2),
				new PointF(-rectW/2,rectH/2),
				new PointF(rectW/2,rectH/2),
				new PointF(rectW/2,rectH/8)
			};

			var pen = UnconnectedSocketPen;
			if (highlight)
			{
                g.FillRectangle(CanConnectSocketFeedbackFill, -rectW / 2, -rectH / 2, rectW, rectH);
				pen = CanConnectSocketFeedbackPen;
			}
			else if(socket.IsConnected) {
                g.FillRectangle(ConnectedSocketFill, -rectW / 2, -rectH / 2, rectW, rectH);
				pen = ConnectedSocketPen;
			}

			g.DrawLines(pen, points);
            
			//See save above
			g.Restore(state);

            ////For debugging ...
            //var b = this.AbsoluteBoundingBox;
            //g.DrawRectangle(new Pen(Color.Blue, 0.01f), (float)b.X, (float)b.Y, (float)b.Width, (float)b.Height);

			base.OnPaintShape(e);
		}

        public override bool HasConnectionPoints
        {
            get
            {
                return true;                
            }
        }

        /// <summary>
        /// Create connection points on the mid point of all 4 sides of the socket rectangle
        /// </summary>
        /// <param name="link"></param>
        public override void EnsureConnectionPoints(LinkShape link)
        {
            //Don't call the base because that creates the default connection points, which we don't want
            //base.EnsureConnectionPoints(link);

            if (this.ConnectionPoints.Count == 4)
                return; //Already created

            this.ConnectionPoints.Clear();
            PointD c = this.AbsoluteBoundingBox.Center;

            //Create a connection point on each edge of the socket
            var points = new PointD[] {
				new PointD(c.X-rectW/2, c.Y),
                new PointD(c.X+rectW/2, c.Y),
                new PointD(c.X, c.Y-rectH/2),
                new PointD(c.X, c.Y+rectH/2)
			};

            //Rotate points according to the socket orientation
            double angle = (((SocketBase)(this.ModelElement)).Definition.Orientation/180.0)*Math.PI;
            var rotatedPoints = from p in points select Rotate(p, c, angle);

            foreach(PointD p in rotatedPoints)
                CreateConnectionPoint(p);
        }

        private static PointD Rotate(PointD p, PointD origin, double angle)
        {
            double x = p.X - origin.X;
            double y = p.Y - origin.Y;

            double xr = x * Math.Cos(angle) - y * Math.Sin(angle);
            double yr = x * Math.Sin(angle) + y * Math.Cos(angle);
            
            return new PointD(xr+origin.X, yr+origin.Y);
        }
            
		/// <summary>
		/// Used during drags to highlight potential connection candidates. See SocketUseShape below ...
		/// </summary>
		public bool Highlight
		{
			get
			{
				return highlight;
			}
			set
			{
				highlight = value;
				Invalidate(true);
			}
		}

		public override bool HasHighlighting
		{
			get
			{
				return false;
			}
		}

		public override bool HasShadow
		{
			get
			{
				return false;
			}
		}


		//We don't want the user moving the sockets around
		public override bool CanMove
		{
			get
			{
				return false;
			}
		}


		public override NodeShape.NodeSides ResizableSides
		{
			get
			{
				return NodeShape.NodeSides.None;
			}
		}

        //This lets user just click and drag a socket to connect, rather than needing to explicitly select the connection tool
        public override MouseAction GetPotentialMouseAction(System.Windows.Forms.MouseButtons mouseButtons, PointD point, DiagramHitTestInfo hitTestInfo)
        {
            var diagram = this.Diagram as GadgeteerDSLDiagram;
            if (diagram != null && !((SocketBase)this.ModelElement).IsConnected)
                return diagram.ConnectAction;

            return base.GetPotentialMouseAction(mouseButtons, point, hitTestInfo);
        }

        /// <summary>
        /// Highlight all potential connections for this shape. We do this with generics because it needs to work
        /// for both SocketShapes and SocketUseShapes. Examples:
        /// 
        /// HighlightPossibleConnections<SocketUse, Socket>();
        /// HighlightPossibleConnections<Socket, SocketUse>();           
        /// </summary>
        /// <typeparam name="S">The type of source of the mouse down</typeparam>
        /// <typeparam name="T">The target type of the potential connection</typeparam>
        protected void HighlightPossibleConnections<S,T>()
            where S : SocketBase   
            where T : SocketBase   
        {
            //Hightlight all compatible sockets
            S source = this.ModelElement as S;
            if (source == null)
                return;

            var targets = Store.ElementDirectory.FindElements<T>(false);

            foreach (var target in targets)
            {                
                    if (!source.CanConnectTo(target))
                        continue;

                var targetShape = PresentationViewsSubject.GetPresentation(target).FirstOrDefault() as SocketShapeBase;
                if (targetShape != null)
                {
                    targetShape.Highlight = true;
                }
            }

        }

	}

	partial class SocketUseShape
	{		
        /// <summary>
        /// Highlight potential connections for this shape
        /// </summary>
		public override void OnMouseDown(DiagramMouseEventArgs e)
		{
			base.OnMouseDown(e);
            HighlightPossibleConnections<SocketUse, Socket>();
		}
	}

    partial class SocketShape
    {
        /// <summary>
        /// Highlight potential connections for this shape
        /// </summary>
        public override void OnMouseDown(DiagramMouseEventArgs e)
        {
            base.OnMouseDown(e);
            HighlightPossibleConnections<Socket, SocketUse>();           
        }
    }

}
