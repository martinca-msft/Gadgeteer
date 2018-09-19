﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using DslModeling = global::Microsoft.VisualStudio.Modeling;
using DslDesign = global::Microsoft.VisualStudio.Modeling.Design;
using DslDiagrams = global::Microsoft.VisualStudio.Modeling.Diagrams;

namespace Microsoft.Gadgeteer.Designer
{
	/// <summary>
	/// DomainClass SocketConnector
	/// Socket connection
	/// </summary>
	[DslDesign::DisplayNameResource("Microsoft.Gadgeteer.Designer.SocketConnector.DisplayName", typeof(global::Microsoft.Gadgeteer.Designer.GadgeteerDSLDomainModel), "Microsoft.Gadgeteer.Designer.GeneratedCode.DomainModelResx")]
	[DslDesign::DescriptionResource("Microsoft.Gadgeteer.Designer.SocketConnector.Description", typeof(global::Microsoft.Gadgeteer.Designer.GadgeteerDSLDomainModel), "Microsoft.Gadgeteer.Designer.GeneratedCode.DomainModelResx")]
	[DslModeling::DomainModelOwner(typeof(global::Microsoft.Gadgeteer.Designer.GadgeteerDSLDomainModel))]
	[global::System.CLSCompliant(true)]
	[DslModeling::DomainObjectId("617632a1-6abc-4627-903b-a67c50a3fd74")]
	public partial class SocketConnector : DslDiagrams::BinaryLinkShape
	{
		#region DiagramElement boilerplate
		private static DslDiagrams::StyleSet classStyleSet;
		private static global::System.Collections.Generic.IList<DslDiagrams::ShapeField> shapeFields;
		private static global::System.Collections.Generic.IList<DslDiagrams::Decorator> decorators;
		
		/// <summary>
		/// Per-class style set for this shape.
		/// </summary>
		protected override DslDiagrams::StyleSet ClassStyleSet
		{
			get
			{
				if (classStyleSet == null)
				{
					classStyleSet = CreateClassStyleSet();
				}
				return classStyleSet;
			}
		}
		
		/// <summary>
		/// Per-class ShapeFields for this shape.
		/// </summary>
		public override global::System.Collections.Generic.IList<DslDiagrams::ShapeField> ShapeFields
		{
			get
			{
				if (shapeFields == null)
				{
					shapeFields = CreateShapeFields();
				}
				return shapeFields;
			}
		}
		
		/// <summary>
		/// Event fired when decorator initialization is complete for this shape type.
		/// </summary>
		public static event global::System.EventHandler DecoratorsInitialized;
		
		/// <summary>
		/// List containing decorators used by this type.
		/// </summary>
		public override global::System.Collections.Generic.IList<DslDiagrams::Decorator> Decorators
		{
			get 
			{
				if(decorators == null)
				{
					decorators = CreateDecorators();
					
					// fire this event to allow the diagram to initialize decorator mappings for this shape type.
					if(DecoratorsInitialized != null)
					{
						DecoratorsInitialized(this, global::System.EventArgs.Empty);
					}
				}
				
				return decorators; 
			}
		}
		
		/// <summary>
		/// Finds a decorator associated with SocketConnector.
		/// </summary>
		public static DslDiagrams::Decorator FindSocketConnectorDecorator(string decoratorName)
		{	
			if(decorators == null) return null;
			return DslDiagrams::ShapeElement.FindDecorator(decorators, decoratorName);
		}
		
		#endregion
		
		#region Connector styles
		/// <summary>
		/// Initializes style set resources for this shape type
		/// </summary>
		/// <param name="classStyleSet">The style set for this shape class</param>
		protected override void InitializeResources(DslDiagrams::StyleSet classStyleSet)
		{
			base.InitializeResources(classStyleSet);
			
			// Line pen settings for this connector.
			DslDiagrams::PenSettings linePen = new DslDiagrams::PenSettings();
			linePen.Color = global::System.Drawing.Color.FromArgb(255, 0, 186, 255);
			classStyleSet.OverridePen(DslDiagrams::DiagramPens.ConnectionLineDecorator, linePen);
			linePen.Width = 0.02f;
			classStyleSet.OverridePen(DslDiagrams::DiagramPens.ConnectionLine, linePen);
			DslDiagrams::BrushSettings lineBrush = new DslDiagrams::BrushSettings();
			lineBrush.Color = global::System.Drawing.Color.FromArgb(255, 0, 186, 255);
			classStyleSet.OverrideBrush(DslDiagrams::DiagramBrushes.ConnectionLineDecorator, lineBrush);
			
			DslDiagrams::BrushSettings textBrush = new DslDiagrams::BrushSettings();
			textBrush.Color = global::System.Drawing.Color.FromKnownColor(global::System.Drawing.KnownColor.DimGray);
			classStyleSet.OverrideBrush(DslDiagrams::DiagramBrushes.ShapeText, textBrush);
		
		}
		
		#endregion
		
		#region Constructors, domain class Id
	
		/// <summary>
		/// SocketConnector domain class Id.
		/// </summary>
		public static readonly new global::System.Guid DomainClassId = new global::System.Guid(0x617632a1, 0x6abc, 0x4627, 0x90, 0x3b, 0xa6, 0x7c, 0x50, 0xa3, 0xfd, 0x74);
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Store where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public SocketConnector(DslModeling::Store store, params DslModeling::PropertyAssignment[] propertyAssignments)
			: this(store != null ? store.DefaultPartitionForClass(DomainClassId) : null, propertyAssignments)
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="partition">Partition where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public SocketConnector(DslModeling::Partition partition, params DslModeling::PropertyAssignment[] propertyAssignments)
			: base(partition, propertyAssignments)
		{
		}
		#endregion
	}
}
