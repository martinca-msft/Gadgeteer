// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;
using System.ComponentModel;
using System.Collections.Generic;

namespace Microsoft.Gadgeteer.Designer
{
    //This is needed to support nested shapes. When we declare these elements as having a custom parent in the model
    //the generated code looks for a FixUpDiagram class with these methods.
    partial class FixUpDiagram
    {
        private ModelElement GetParentForSocket(Socket childElement)
        {
            return childElement.GadgeteerHardware;
        }

        private ModelElement GetParentForSocketUse(SocketUse childElement)
        {
            return childElement.Module;
        }
    }


    /// <summary>
    /// Diagram customizations:
    /// 
    /// - Undo the highlight of potential socket connections during a connect operation
    /// - Set shape icon and size in OnChildConfiguring
    /// </summary>
    public partial class GadgeteerDSLDiagram : IServiceProvider
    {        
        private SocketConnectionToolConnectAction connectAction;
        private CustomOperations operationsOverride;
        private List<ToolboxItemFilterAttribute> customToolboxFilters = new List<ToolboxItemFilterAttribute>();

        public override void OnInitialize()
        {
            base.OnInitialize();
            connectAction = new SocketConnectionToolConnectAction(this);
            connectAction.MouseActionDeactivated += (sender, e) => this.OnMouseActionDeactivated();
            
            operationsOverride = new CustomOperations(this, this);
        }

        //This connectAction is used by the SocketUseShape to allow connections using the standard selection pointer
        internal MouseAction ConnectAction { get { return connectAction; } }

        protected override void OnMouseActionDeactivated()
        {
            base.OnMouseActionDeactivated();            
            RemoveSocketHighlights();
        }

        private void RemoveSocketHighlights()
        {
            //Get all SocketShapes and SocketUseShapes
            var socketShapes = Store.ElementDirectory.FindElements<SocketShapeBase>(true /*Get descendants*/);
            foreach (var ss in socketShapes)
                ss.Highlight = false;                    
        }

        protected override void OnChildConfiguring(ShapeElement child, bool createdDuringViewFixup)
        {
            base.OnChildConfiguring(child, createdDuringViewFixup);
            var shape = child as GadgeteerHardwareShape;            

            //Make sure pick up the size/icon from the definition
            if (shape != null)
                shape.OnDefinitionChanged();
        }

        public void AddToolboxFilter(ToolboxItemFilterAttribute filter)
        {
            customToolboxFilters.Add(filter);
        }

        public override System.Collections.ICollection TargetToolboxItemFilterAttributes
        {
            get
            {
                return customToolboxFilters;
            }
        }
                

        public override DesignSurfaceElementOperations ElementOperations
        {
            get
            {
                return operationsOverride;
            }
        }

        /// <summary>
        /// A customization of DesignSurfaceElementOperations to prevent the "Delete" operation from being displayed
        /// for Sockets
        /// </summary>
        class CustomOperations : DesignSurfaceElementOperations
        {
            public CustomOperations(Diagram d, IServiceProvider sp) : base(sp, d) { }

            public override bool CanDelete(ModelElement element, params System.Guid[] domainRolesToNotPropagate)
            {
                if(element is SocketBase)
                    return false;

                return base.CanDelete(element, domainRolesToNotPropagate);
            }

            public override bool CanDelete(System.Collections.Generic.IEnumerable<ModelElement> elements, params Guid[] domainRolesToNotPropagate)
            {
                foreach (ModelElement m in elements)
                    if (m is SocketBase)
                        return false;

                return base.CanDelete(elements, domainRolesToNotPropagate);
            }
            
        }
    }

}
