// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Markup;
using DslShell = global::Microsoft.VisualStudio.Modeling.Shell;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Customize the doc view to:
    /// 
    /// - Show the toolbox when the diagram is opened
    /// </summary>
    partial class GadgeteerDSLDocView
    {
        private const string ViewToolBoxCommand = "View.Toolbox";
        private const string ViewOutputWindowCommand = "View.Output";
        private const string DebugStartCommand = "Debug.Start";

        private ElementHost powerOverlayHost;
        private DocViewPowerOverlay powerOverlay;

        public bool IsPowerOverlayVisible
        {
            get { return this.powerOverlayHost != null && this.powerOverlayHost.Visible; }
            set
            {
                if (this.powerOverlayHost != null && this.powerOverlay != null)
                {
                    this.powerOverlayHost.Visible = value;
                }
            }
        }
        public bool IsPowerOverlayAvailable
        {
            get { return this.powerOverlay != null && this.powerOverlay.HasAnythingToShow; }
        }

        public override DslShell.VSDiagramView CreateDiagramView()
        {
            DslShell.VSDiagramView view = base.CreateDiagramView();

            CreateOverlay(view);
                                 
            return view;
        }

        private void CreateOverlay(DslShell.VSDiagramView view)
        {
            this.powerOverlayHost = new ElementHost
            {
                Left = 10,
                Top = 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                AutoSize = true,
                Child = this.powerOverlay = new DocViewPowerOverlay(),
                BackColor = System.Drawing.Color.White,
                BackColorTransparent = false,
                Visible = false
            };

            view.Controls.Add(powerOverlayHost);
            this.powerOverlayHost.BringToFront();
        }

        protected override void Initialize()
        {
            base.Initialize();
            OpenToolbox();
        }

        private void OpenToolbox() { ExecuteDTECommand(ViewToolBoxCommand); }
        private void OpenOutput() { ExecuteDTECommand(ViewOutputWindowCommand); }
        private void Run() { ExecuteDTECommand(DebugStartCommand); }

        protected void ExecuteDTECommand(string command)
        {
            var dte = this.GetService(typeof(DTE)) as DTE;
            if (dte != null) dte.ExecuteCommand(command);
        }

        internal void UpdateFromStore(Microsoft.VisualStudio.Modeling.Store store)
        {
            try
            {
                DocViewPowerOverlay.UpdateFrom(store.ElementDirectory.AllElements, this.powerOverlay);
            }
            catch
            {
                this.IsPowerOverlayVisible = false;
            }
        }

        
        // Show power requirements for the selected elements only.
        // This, however, suggests that selecting a route will give you the power estimate for this route, which is not the case.
        //protected override void OnSelectionChanged(System.EventArgs e)
        //{
        //    try
        //    {
        //        GadgeteerHardwareShape[] shapes = this.SelectedElements.OfType<GadgeteerHardwareShape>().ToArray();
        //        if (shapes.Length < 1)
        //        {
        //            UpdateFromStore(this.CurrentDiagram.Store);
        //        }
        //        else
        //        {
        //            DocViewPowerOverlay.UpdateFrom(from shape in shapes
        //                                           select shape.ModelElement, this.powerOverlay);
        //        }
        //    }
        //    catch
        //    {
        //        this.IsPowerOverlayVisible = false;
        //    }
        //}
    }
}
