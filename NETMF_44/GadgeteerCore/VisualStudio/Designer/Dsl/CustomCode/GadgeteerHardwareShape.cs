// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using EnvDTE;
using Microsoft.Gadgeteer.Designer.Definitions;
using Microsoft.VisualStudio.Modeling.Diagrams;

namespace Microsoft.Gadgeteer.Designer
{

    
    //Can't implement IDisposable because the generated code won't call dispose on the shape. Instead, "icon" gets disposed in the 
    //OnShapeRemoved override below
    /// <summary>
    /// Base class for module and mainboard shapes. Deals with drawing the right icon and sizing the shapes according to the corresponding
    /// xml definition.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]        
    public partial class GadgeteerHardwareShape
    {
        //Image to display in the shape (if provided in the module/mainboard definition)
        private Bitmap image;

        //Need the DPI to resize the image properly. We grab the values in OnPaintShape(). Store these in static
        //variables because when a module gets dropped the GetImage method gets called before the firt OnPaintShape()
        static float dpiX;
        static float dpiY;

        /// <summary>
        /// Some modules don't provide an image. If there's no image we want to show the rounded rectangle shape
        /// with a dark background. But if there is we omit the ouline/shadow/background because it doesn't
        /// look well with the image unless the image has all the transparency set correctly, which some module
        /// manufacturers won't do.
        /// </summary>
        bool imageExists = true;


        public override NodeShape.NodeSides ResizableSides
        {
            get
            {
                return NodeShape.NodeSides.None;
            }
        }

        public override SizeD MinimumSize
        {
            get
            {
                GadgeteerHardware hw = this.ModelElement as GadgeteerHardware;
                if (hw != null && hw.GadgeteerPartDefinition != null)
                    return new SizeD(ToInches(hw.GadgeteerPartDefinition.BoardWidth),
                                     ToInches(hw.GadgeteerPartDefinition.BoardHeight));
                else
                    return base.MinimumSize;
                                     
            }
        }

        public override double GridSize
        {
            get
            {
                return 0.02;
            }
            set
            {
                base.GridSize = value;
            }
        }

        public override SizeD MaximumSize
        {
            get
            {
                return this.MinimumSize;
            }
        }


        public override void OnPaintShape(DiagramPaintEventArgs e)
        {
            //Grab the dpi values here because we need them to resize the image correctly in GetImage()
            dpiX = e.Graphics.DpiX;
            dpiY = e.Graphics.DpiY;

            base.OnPaintShape(e);
        }


        /// <summary>
        /// Override to place sockets in the positions specified by the XML file
        /// </summary>
        /// <param name="child"></param>
        /// <param name="createdDuringViewFixup"></param>
        protected override void OnChildConfiguring(ShapeElement child, bool createdDuringViewFixup)
        {
            base.OnChildConfiguring(child, createdDuringViewFixup);
            var socketShape = child as SocketShapeBase;

            if(socketShape!=null)   
                LayoutSocket(socketShape);
        }

        private void LayoutSocket(SocketShapeBase socketShape)
        {
            var socket = socketShape.ModelElement as SocketBase;
            if (socket != null)
            {
                var def = socket.Definition;
                if (def != null)
                {
                    socketShape.Location = new PointD(ToInches(def.Left) - socketShape.Size.Width / 2, ToInches(def.Top) - socketShape.Size.Height / 2);
                }
            }
        }

        public override bool HasShadow
        {
            get
            {
                return image == null; //See comment on imageExists declaration
            }
        }

        public override bool HasOutline
        {
            get
            {
                return image == null; //See comment on imageExists declaration               
            }
        }

        public override bool HasFilledBackground
        {
            get
            {
                return image == null; //See comment on imageExists declaration
            }
        }       

        private static double ToInches(double mm) { return mm / 25.4; }

        
        public bool? HasHelp
        {
            get; internal set;
        }

        internal Bitmap GetImage()
        {            
            if (!imageExists)
                return null;

            //This can happen if the mouse is over the shape as it is loading. In this case the hit test code tries to figure out
            //the shape bounds, which causes a call to the IconImageField.GetDisplayImage. All of this happens before the first OnPaintShape()
            //so we don't have the dpi yet
            if (dpiX == 0 || dpiY == 0)
                return null;

            if (image == null)
            {
                var hardware = ModelElement as GadgeteerHardware;
                if (hardware != null)
                {
                    GadgeteerPart part = hardware.GadgeteerPartDefinition;
                    if (part != null)
                    {
                        if (imageExists = IsValidImage(part.Image))
                        {
                            using (var bmp = new Bitmap(part.Image))
                            {
                                try
                                {
                                    image = new Bitmap(bmp, (int)(ToInches(part.BoardWidth) * dpiX), (int)(ToInches(part.BoardHeight) * dpiY));
                                }
                                catch (ArgumentException e)
                                {
                                    //log it and eat it, in case we missed something. otherwise this can crash the designer
                                    Log.WriteError(e);
                                }
                            }
                        }
                    }
                }
            }
            
            return image;
        }

        static bool IsValidImage(string imagePath)
        {
            if (!File.Exists(imagePath))
                return false;

            return (new FileInfo(imagePath)).Length > 0;

        }

        protected override void InitializeShapeFields(IList<Microsoft.VisualStudio.Modeling.Diagrams.ShapeField> shapeFields)
        {
            try
            {
                base.InitializeShapeFields(shapeFields);
                shapeFields.Clear();
                shapeFields.Add(new IconImageField());
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// A simple image field that fetches from image from the icon defined in the xml 
        /// </summary>
        class IconImageField : ImageField
        {
            public IconImageField() : base("icon") {}

            public override Image GetDisplayImage(ShapeElement parentShape)
            {
                var ms = parentShape as GadgeteerHardwareShape;                
                
                if (ms != null)
                {
                    return ms.GetImage();
                }
                return null;
            }
        }

        internal void OnDefinitionChanged()
        {
            image = null;
            Size = this.MinimumSize;
            Invalidate(true);
        }

        public override void OnShapeRemoved()
        {
            base.OnShapeRemoved();
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
        }        
    }

    
}
