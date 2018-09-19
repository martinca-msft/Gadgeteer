// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

//These set of classes are used to deserialize the GadgeteerHardware.xml files. They need to be kept in sync with
//GadgeteerHardware.xsd
namespace Microsoft.Gadgeteer.Designer.Definitions
{
    [XmlRoot(Namespace="http://schemas.microsoft.com/Gadgeteer/2011/Hardware")]
    public class GadgeteerDefinitions
    {
        [XmlArray]
        public Collection<ModuleDefinition> ModuleDefinitions { get; set; }
        [XmlArray]
        public Collection<MainboardDefinition> MainboardDefinitions { get; set; }
    }

	public class GadgeteerPart
	{
		[XmlAttribute]
		public string Name { get; set; }
		[XmlAttribute]
		public string Type { get; set; }
		[XmlAttribute]
		public string HardwareVersion { get; set; }
		[XmlAttribute]
		public string Image { get; set; }
		[XmlAttribute]
		public double BoardWidth { get; set; }
		[XmlAttribute]
		public double BoardHeight { get; set; }
		[XmlAttribute]
		public string MinimumGadgeteerCoreVersion { get; set; }

        [XmlAttribute]
        public string HelpUrl { get; set; }

		[XmlArray]
		public Collection<ProvidedSocket> ProvidedSockets { get; set; }
		
		[XmlArray]
		public Collection<Assembly> Assemblies { get; set; }

		[XmlArray]
		public Collection<Assembly> LibrariesProvided { get; set; }    

        public IEnumerable<string> SupportedMicroframeworkVersions
        {
            get
            {
               return Assemblies.Select(a => a.MFVersion).Distinct();
            }
        }
    }

    [XmlType]
    public class ModuleDefinition : GadgeteerPart
    {
		[XmlAttribute]
        public Guid UniqueId { get; set; }
		[XmlAttribute]
		public string Manufacturer { get; set; }
		[XmlAttribute]
		public string Description { get; set; }
		[XmlAttribute]
        public string InstanceName { get; set; }
		[XmlAttribute]
		public bool ModuleSuppliesPower { get; set; }

        [XmlArray]
        public Collection<SocketUse> Sockets{ get; set; }

		[XmlArray]
		public Collection<Assembly> ExtraLibrariesRequired { get; set; }

    }

    [XmlType]
    public class MainboardDefinition : GadgeteerPart
    {
    }

    public abstract class SocketBase
    {
        [XmlAttribute]
        public double Orientation { get; set; }
        [XmlAttribute]
        public double Left { get; set; }
        [XmlAttribute]
        public double Top { get; set; }
    }

    [XmlType]
	public class ProvidedSocket : SocketBase
    {
		[XmlAttribute]
		public string Label { get; set; }
		[XmlAttribute]
		public string ConstructorParameter { get; set; }

        [XmlArrayItem("Type")]
        public Collection<string> Types { get; set; }

		[XmlArrayItem("SharedPinMap")]
		public Collection<SharedPinMap> SharedPinMaps { get; set; }

    }

    [XmlType("Socket")]
    public class SocketUse : SocketBase
    {
		[XmlAttribute]
		public string TypesLabel { get; set; }
		[XmlAttribute]
        public int ConstructorOrder { get; set; }
		[XmlAttribute]
		public bool Optional { get; set; }

        [XmlArrayItem(ElementName="Type", Type=typeof(SingleSocketType))]
        [XmlArrayItem(ElementName="CompositeType", Type=typeof(CompositeType))]
        public Collection<SocketType> Types { get; set; }

        [XmlArray]
        public Collection<Pin> Pins { get; set; }
    }

    [XmlType]
    public class Pin 
    {
        [XmlAttribute]
        public bool Shared { get; set; }
        [XmlText]
        public int Value { get; set; }
    }

	[XmlType]
	public class SharedPinMap
	{
		[XmlAttribute]
		public int SocketPin { get; set; }
		[XmlAttribute]
		public string NetId { get; set; }
        [XmlAttribute]
        public bool SharedOnly { get; set; }
	}
    

    public abstract class SocketType { }

    [XmlType]
    public class CompositeType : SocketType
    {        
        [XmlArrayItem("Type")]
        public Collection<SingleSocketType> Types { get; set; }
    }

    [XmlType("Type")]
    public class SingleSocketType : SocketType
    {
        [XmlText]
        public string Value { get; set; }
    }

    [XmlType]
    public class Assembly
    {
        [XmlAttribute]
        public string MFVersion { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ErrorMessage { get; set; }
    }    

}