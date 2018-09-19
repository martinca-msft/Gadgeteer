// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Gadgeteer.Designer.Definitions;
using Microsoft.Gadgeteer.Designer;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GadgeteerDesignerUnitTests
{
    [TestClass]
    public class DependencyTests : ModelTest
    {
        private const string AssemblyA = "AssemblyA";
        private const string AssemblyB = "AssemblyB";

        ModuleDefinition reqAB = CreateModuleDef(new[] { AssemblyA, AssemblyB}, new string[0]);
        ModuleDefinition provA1 = CreateModuleDef(new string[0], new[] { AssemblyA });
        ModuleDefinition provA2 = CreateModuleDef(new string[0], new[] { AssemblyA });
        ModuleDefinition provB = CreateModuleDef(new string[0], new[] { AssemblyB });

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            
            var moDefs = new List<ModuleDefinition>() {reqAB, provA1, provA2, provB};

            GadgeteerDefinitionsManager.Instance.Initialize(new MainboardDefinition[0], moDefs);
        }

        

        [TestMethod]
        public void TestMissingDependencies()
        {
            //To avoid rules firing
            DoInTransactionDontCommit(() =>
                {
                    Module m = CreateModule(reqAB);
                    Module pA1 = CreateModule(provA1);
                    Module pA2 = CreateModule(provA2);
                    Module pB = CreateModule(provB);

                    this.GadgeteerModel.GadgeteerHardware.Add(m);
                    Action<string, bool> assertMissing = (asm, missing) => Assert.AreEqual(missing, m.GetMissingDependencyErrors().Any(e => e.Contains(asm)), asm+" should "+(missing? "": "not")+" be missing");

                    assertMissing(AssemblyA, true);
                    assertMissing(AssemblyB, true);

                    this.GadgeteerModel.GadgeteerHardware.Add(pA1);

                    assertMissing(AssemblyA, false);
                    assertMissing(AssemblyB, true);
                    
                    this.GadgeteerModel.GadgeteerHardware.Add(pB);

                    assertMissing(AssemblyA, false);
                    assertMissing(AssemblyB, false);                    

                    this.GadgeteerModel.GadgeteerHardware.Remove(pA1);

                    assertMissing(AssemblyA, true);
                    assertMissing(AssemblyB, false);
                });
        }


        Module CreateModule(ModuleDefinition def)
        {
            Module m = new Module(Store) { ModuleDefinitionId = def.UniqueId };
            Debug.Assert(m.GadgeteerPartDefinition == def);
            return m;
        }

        static ModuleDefinition CreateModuleDef(IEnumerable<string> requiredAssemblies, IEnumerable<string> providedAssemblies)
        {
            Func<IEnumerable<string>, IList<Assembly>> toAssemblyList = source =>
                                                                       source.Select(asmName => new Assembly() { Name = asmName }).ToList();

            var d = new ModuleDefinition() { UniqueId = Guid.NewGuid() };
            d.ExtraLibrariesRequired = new Collection<Assembly>(toAssemblyList(requiredAssemblies));
            d.LibrariesProvided = new Collection<Assembly>(toAssemblyList(providedAssemblies));

            return d;
        }
    }
}
