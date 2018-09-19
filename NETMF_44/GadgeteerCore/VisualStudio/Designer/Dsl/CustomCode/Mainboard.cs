// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Gadgeteer.Designer.Definitions;
using Microsoft.VisualStudio.Modeling;


namespace Microsoft.Gadgeteer.Designer
{
    partial class Mainboard
    {
        private const string UnknownMainboard = "UNKNOWN_Mainboard";
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(MainboardDefinition));
        
        /// <summary>
        /// Use a default empty definition in cases where we can't find a proper MainboardDefinition for this mainboard
        /// </summary>
        private static readonly MainboardDefinition EmptyDefinition = new MainboardDefinition()
        {
            BoardHeight = 30,
            BoardWidth = 30,
            ProvidedSockets = new Collection<ProvidedSocket>()
        };

        public Mainboard(Store store, bool createdByNewModel, params PropertyAssignment[] propertyAssignments)
            : base(store != null ? store.DefaultPartitionForClass(DomainClassId) : null, propertyAssignments)
        {
            CreatedByNewModel = createdByNewModel;
        }

        /// <summary>
        /// Was this Mainboard programmatically created by the designer or was it deserialized from a file during Load?
        /// </summary>
        public bool CreatedByNewModel { get;  private set; }

        internal override GadgeteerPart GadgeteerPartDefinition
        {
            get
            {
                var boardDefs = GadgeteerDefinitionsManager.Instance.Mainboards;

                if (!string.IsNullOrWhiteSpace(this.Name))
                    boardDefs = boardDefs.Where(bd => bd.Name == this.Name);

                var ret = boardDefs.FirstOrDefault();

                if (ret == null)
                    return CachedPartDefinition ?? EmptyDefinition;
                
                return ret;
            }
        }

        protected override XmlSerializer CachedDefinitionSerializer
        {
            get { return serializer; }
        }

        //We need a public way of accessing the definition for code generation purposes, the T4 template can't access internal members
        public string MainboardDefinitionTypeName
        {
            get
            {
                var d = GadgeteerPartDefinition as MainboardDefinition;
                return d != null ? d.Type ?? UnknownMainboard : UnknownMainboard;
            }
        }

        public bool HasDefinitions
        {
            get
            {                
                if (GadgeteerPartDefinition == EmptyDefinition)
                    return false;

                return Sockets.All(s => s.HasDefinition) &&
                       this.GadgeteerModel.Modules.All(m => m.HasDefinitions);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", typeof(Mainboard).Name, this.Name);
        }

        //Prevent pasting of sockets
        protected override bool CanMerge(ProtoElementBase rootElement, ElementGroupPrototype elementGroupPrototype)
        {
            return false;
        }
    } 
}
