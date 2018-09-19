// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using IEnumerable = System.Collections.IEnumerable;

//These set of classes are used to deserialize the GadgeteerHardware.xml files. They need to be kept in sync with
//GadgeteerHardware.xsd
namespace Microsoft.Gadgeteer.Designer.Definitions
{
    public abstract class Validatable
    {
        private List<string> _errorList;
        public ReadOnlyCollection<string> GetErrors()
        {
            if (_errorList == null)
            {
                _errorList = new List<string>();
                GetErrors(_errorList);
            }

            return _errorList.AsReadOnly();
        }

        protected internal virtual void GetErrors(IList<string> errorList)
        {

        }
        
        protected void GetMemberErrors(IEnumerable<Validatable> validatables, IList<string> errorList)
        {
            if (validatables != null)
                foreach (var validatable in validatables)
                    validatable.GetErrors(errorList);
        }

        public bool HasErrors
        {
            get { return GetErrors().Count > 0; }
        }
    }

    [XmlRoot(Namespace="http://schemas.microsoft.com/Gadgeteer/2011/Hardware")]
    public class GadgeteerDefinitions : Validatable
    {
        [XmlArray]
        public Collection<ModuleDefinition> ModuleDefinitions { get; set; }
        [XmlArray]
        public Collection<MainboardDefinition> MainboardDefinitions { get; set; }

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);
            int definitionCount = 0;

            if (ModuleDefinitions != null)
            {
                definitionCount += ModuleDefinitions.Count;
                GetMemberErrors(ModuleDefinitions, errorList);
            }

            if (MainboardDefinitions != null)
            {
                definitionCount += MainboardDefinitions.Count;
                GetMemberErrors(MainboardDefinitions, errorList);
            }

            if (definitionCount < 1)
                errorList.Add("No hardware defined.");
        }
    }

	public class GadgeteerPart : Validatable
	{
		[XmlAttribute]
		public string Name { get; set; }
		[XmlAttribute]
		public string Manufacturer { get; set; }
		[XmlAttribute]
		public string Description { get; set; }
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
        public Collection<PowerRequirements> Power { get; set; }
		[XmlArray]
		public Collection<ProvidedSocket> ProvidedSockets { get; set; }
		[XmlArray]
		public Collection<Assembly> Assemblies { get; set; }
		[XmlArray]
		public Collection<Assembly> LibrariesProvided { get; set; }    

        public IEnumerable<string> SupportedMicroframeworkVersions
        {
            get { return Assemblies.Select(a => a.MFVersion).Distinct(); }
        }

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);
            Version version;
            Uri uri;

            if (string.IsNullOrEmpty(Name))
                errorList.Add("Missing or empty Name.");

            if (this is ModuleDefinition)
            {
                if (string.IsNullOrWhiteSpace(Manufacturer))
				{
					errorList.Add("Missing Manufacturer.");
					Manufacturer = "(no manufacturer)";
				}

				if (Description == null)
					errorList.Add("Missing Description.");
            }

            if (string.IsNullOrEmpty(Type))
                errorList.Add("Missing or empty Type.");

            if (!Version.TryParse(HardwareVersion, out version))
                errorList.Add("Missing or invalid HardwareVersion.");

            if (BoardWidth < 1.0)
                errorList.Add("Invalid BoardWidth.");
            
            if (BoardHeight < 1.0)
                errorList.Add("Invalid BoardHeight.");            

            if (!Version.TryParse(HardwareVersion, out version))
                errorList.Add("Missing or invalid MinimumGadgeteerCoreVersion.");

            if (!string.IsNullOrEmpty(HelpUrl) && !Uri.TryCreate(HelpUrl, UriKind.RelativeOrAbsolute, out uri))
                errorList.Add("Invalid HelpUrl.");

            if (Assemblies == null || Assemblies.Count < 1)
                errorList.Add("No assemblies defined.");
            else
                GetMemberErrors(Assemblies, errorList);

            GetMemberErrors(Power, errorList);
            GetMemberErrors(ProvidedSockets, errorList);
            GetMemberErrors(LibrariesProvided, errorList);
        }

        [XmlIgnore]
        public string ErrorMessage;
    }

    [XmlType]
    public class ModuleDefinition : GadgeteerPart
    {
		[XmlAttribute]
        public Guid UniqueId { get; set; }
		[XmlAttribute]
        public string InstanceName { get; set; }
		[XmlAttribute]
		public bool ModuleSuppliesPower { get; set; }

        [XmlArray]
        public Collection<SocketUse> Sockets{ get; set; }

		[XmlArray]
		public Collection<Assembly> ExtraLibrariesRequired { get; set; }

        public bool HasAllRequiredLibraries(ICollection<string> availableLibraries, string mfVersion)
        {
            if (ExtraLibrariesRequired.Count < 1)
                return true;

            foreach (Assembly libraryRequired in ExtraLibrariesRequired)
            {
                if (libraryRequired.MFVersion != mfVersion)
                    continue;

                if (!availableLibraries.Contains(libraryRequired.Name))
                    return false;
            }

            return true;
        }

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);

            if (string.IsNullOrWhiteSpace(InstanceName))
                errorList.Add("Missing or empty InstanceName.");

            GetMemberErrors(Sockets, errorList);
            GetMemberErrors(ExtraLibrariesRequired, errorList);
        }

    }

    [XmlType]
    public class MainboardDefinition : GadgeteerPart
    {

    }

    [XmlType]
    public class PowerRequirements : Validatable
    {
        [XmlAttribute]
        public double Voltage { get; set; }

        [XmlElement]
        public double TypicalCurrent { get; set; }

        [XmlElement]
        public double MaximumCurrent { get; set; }

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);

            if (Voltage == 0)
                errorList.Add("Power requirements have zero Voltage.");

            if (TypicalCurrent < 0)
                errorList.Add("Power requirements have negative TypicalCurrent.");

            if (MaximumCurrent < 0)
                errorList.Add("Power requirements have negative MaximumCurrent.");
        }
    }

    public abstract class SocketBase : Validatable
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

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);

            if (Label == null)
                errorList.Add("Missing socket label.");

            if (string.IsNullOrWhiteSpace(ConstructorParameter))
                errorList.Add("Missing or empty socket ConstructorParameter.");

            if (Types != null)
                foreach (var item in Types)
                    if (item == null || item.Length != 1)
                    {
                        errorList.Add("At least one socket has invalid Type.");
                        break;
                    }

            GetMemberErrors(SharedPinMaps, errorList);
        }
    }

    [XmlType("Socket")]
    public class SocketUse : SocketBase
    {
        private const char UniqueSeparator = '`';

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

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);

            if (TypesLabel == null)
                errorList.Add("Missing socket TypesLabel.");

            else if (TypesLabel.EndsWith(UniqueSeparator.ToString()))
                errorList.Add("Socket TypesLabel cannot end with '" + UniqueSeparator + "'.");

            GetMemberErrors(Types, errorList);
            // Pins cannot be wrong
        }

        public string UniqueLabel
        {
            get { return TypesLabel + UniqueSeparator + ConstructorOrder; }
        }
    }

    [XmlType]
    public class Pin 
    {
        [XmlAttribute]
        public bool Shared { get; set; }

        [XmlAttribute]
        public string NetId { get; set; }

        [XmlText]
        public int Value { get; set; }
    }

	[XmlType]
	public class SharedPinMap : Validatable
	{
		[XmlAttribute]
		public int SocketPin { get; set; }
		[XmlAttribute]
		public string NetId { get; set; }
        [XmlAttribute]
        public bool SharedOnly { get; set; }

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);

            if (NetId == null)
                errorList.Add("Missing shared pin map NetId.");
        }
	}
    

    public abstract class SocketType : Validatable { }

    [XmlType]
    public class CompositeType : SocketType
    {        
        [XmlArrayItem("Type")]
        public Collection<SingleSocketType> Types { get; set; }

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);

            GetMemberErrors(Types, errorList);
        }
    }

    [XmlType("Type")]
    public class SingleSocketType : SocketType
    {
        [XmlText]
        public string Value { get; set; }

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);

            if (Value == null || Value.Length != 1)
                errorList.Add("Missing or invalid socket Type.");
        }
    }

    [XmlType]
    public class Assembly : Validatable
    {
        [XmlAttribute]
        public string MFVersion { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ErrorMessage { get; set; }

        protected internal override void GetErrors(IList<string> errorList)
        {
            base.GetErrors(errorList);

            if (string.IsNullOrWhiteSpace(MFVersion))
                errorList.Add("Missing or invalid assembly MFVersion.");

            if (string.IsNullOrWhiteSpace(Name))
                errorList.Add("Missing or invalid assembly name.");
        }
    }
}