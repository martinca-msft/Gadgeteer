// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Gadgeteer.Designer.Definitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Base class for modules and mainboards
    /// </summary>
    partial class GadgeteerHardware
    {
        private GadgeteerPart cachedPartDefinition;

        /// <summary>
        /// Fetch the definition for this element. This method must not return null.
        /// </summary>
        internal abstract GadgeteerPart GadgeteerPartDefinition { get; }

        protected abstract XmlSerializer CachedDefinitionSerializer { get; }

        /// <summary>
        /// Serialize the part definition to save it into the module. This allows us to correctly display diagrams on systems with certain
        /// modules/mainboards missing
        /// </summary>
        internal void CacheDefinition()
        {
            GadgeteerPart part = GadgeteerPartDefinition;
            if (part == null)
                return;

            using (var sw = new StringWriter())
            {
                CachedDefinitionSerializer.Serialize(sw, part);
                this.CachedDefinition = sw.ToString();
            }
        }

        /// <summary>
        /// Fetches the cached part definition, deserializing it from the CachedDefinition property on demand
        /// </summary>
        internal GadgeteerPart CachedPartDefinition
        {
            get
            {
                if (cachedPartDefinition == null && !string.IsNullOrWhiteSpace(this.CachedDefinition))
                {
                    using (var reader = new StringReader(this.CachedDefinition))
                    {
                        cachedPartDefinition = CachedDefinitionSerializer.Deserialize(reader) as GadgeteerPart;
                    }
                }
                return cachedPartDefinition;
            }
        }

        /// <summary>
        /// Create sockets based on the associated definition
        /// </summary>
        internal void CreateSockets()
        {
            GadgeteerPart part = this.GadgeteerPartDefinition;

            if (part == null)
                return;

            var socketsCopy = Sockets.ToArray();
            foreach (Socket s in socketsCopy)
                s.Delete();

            Sockets.Clear();

            var providedSockets = part.ProvidedSockets;
            if (providedSockets.Count == 0)
                return;

            foreach (var providedSocket in providedSockets)
            {
                var s = new Socket(this.Store) { Label = providedSocket.Label };
                this.Sockets.Add(s);
            }
        }
    }
}
