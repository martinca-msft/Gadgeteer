// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Modeling;
using Microsoft.Gadgeteer.Designer;
using Microsoft.Gadgeteer.Designer.Definitions;
using System.Collections.ObjectModel;

namespace GadgeteerDesignerUnitTests
{
    /// <summary>
    /// Base class for testing with the GadgeteerDSL
    /// </summary>
    [TestClass]    
    public abstract class ModelTest
    {
        protected Store Store { get; private set; }
        protected GadgeteerModel GadgeteerModel { get; private set; }

        [TestInitialize]
        public virtual void Initialize()
        {
            this.Store = new Store(typeof(GadgeteerDSLDomainModel));

            DoInTransaction(() => this.GadgeteerModel = new GadgeteerModel(Store));
        }

        [TestCleanup]
        public virtual void Cleanup()
        {
            this.GadgeteerModel = null;
            this.Store.Dispose();
            this.Store = null;            
        }

        protected void DoInTransaction(Action action)
        {
            using (var tx = this.Store.TransactionManager.BeginTransaction())
            {
                action();
                tx.Commit();
            }
        }

        protected void DoInTransactionDontCommit(Action action)
        {
            using (var tx = this.Store.TransactionManager.BeginTransaction())
            {
                action();                
            }
        }

		public static MainboardDefinition CreateMainboardDef(string name, Collection<ProvidedSocket> providedSockets)
		{
			var mbDef = new MainboardDefinition();
			mbDef.Name = name;
			mbDef.ProvidedSockets = providedSockets;
			return mbDef;
		}

		public static ModuleDefinition CreateModuleDef(Guid id, Collection<Microsoft.Gadgeteer.Designer.Definitions.SocketUse> sockets)
		{
			var moDef = new ModuleDefinition();
			moDef.UniqueId = id;
			moDef.Sockets = sockets;
			return moDef;
		}
        

		public static ModuleDefinition CreateModuleDef(Guid id, string socketUseLabel, string csvSingleTypes, bool optional)
		{
			var sus = new Collection<Microsoft.Gadgeteer.Designer.Definitions.SocketUse>();
			var su = CreateSocketUseDef(socketUseLabel, csvSingleTypes.Split(',').ToList<string>(), null);
			su.Optional = optional;
			sus.Add(su);
			return CreateModuleDef(id, sus);
		}

		public static Microsoft.Gadgeteer.Designer.Definitions.SocketUse CreateSocketUseDef(string label, IEnumerable<string> singleValues, IEnumerable<IEnumerable<string>> compositeValues)
		{
			var su = new Microsoft.Gadgeteer.Designer.Definitions.SocketUse();
			su.TypesLabel = label;
			su.Types = new Collection<SocketType>();

			if (singleValues != null)
			{
				foreach (var value in singleValues)
					su.Types.Add(new SingleSocketType() { Value = value });
			}

			if (compositeValues != null)
			{
				foreach (var cType in compositeValues)
				{
					var c = new CompositeType();
					c.Types = new Collection<SingleSocketType>();
					foreach (var value in cType)
						c.Types.Add(new SingleSocketType() { Value = value });
				}
			}

			return su;
		}


	}


    [TestClass]
    public class ModuleSortingTest : ModelTest    
    {
        /// <summary>
        /// Connects a set of modules as:
        /// 
        /// Mainboard --> M1 --> M4 --> M5
        ///           --> M2 --> M3
        /// 
        /// And verifies the code generation order is M1,M4,M5,M2,M3
        /// </summary>
        [TestMethod]
        public void TestSortedModules()
        {
            Mainboard mb = GadgeteerModel.Mainboard;
            DoInTransaction(() =>
            {
                Assert.AreEqual(0, GadgeteerModel.SortModulesInCodeGenerationOrder().Count(),
                                "Empty test failed");


                AddSockets(mb, 3);

                Module m1 = CreateModule();
                Module m2 = CreateModule();
                Module m3 = CreateModule();
                Module m4 = CreateModule();
                Module m5 = CreateModule();

                //Connect m2 with to sockets to the mainboard
                var su = new Microsoft.Gadgeteer.Designer.SocketUse(Store);
                m2.SocketUses.Add(su);
                su.Socket = mb.Sockets[2];

                Connect(mb, m1, 0);
                Connect(mb, m2, 1);
                Connect(m1, m4, 1);
                Connect(m4, m5, 0);
                Connect(m2, m3, 0);

                var expectedOrder = new[] { m1, m4, m5, m2, m3 };
                var result = GadgeteerModel.SortModulesInCodeGenerationOrder();
                bool equal = expectedOrder.SequenceEqual(result);

                Assert.IsTrue(equal, "The sequence did not result in the expected order");
            });
                       
        }

        private static void Connect(GadgeteerHardware source, Module consumer, int providedSocketIndex)
        {
            consumer.SocketUses[0].Socket = source.Sockets[providedSocketIndex];
        }

        private void AddSockets(GadgeteerHardware hw, int sockets)
        {
            if (sockets == 0) return; ;
            hw.Sockets.Add(new Socket(Store));
            AddSockets(hw, sockets-1);
        }
        

        private Module CreateModule()
        {
            var m1 = new Module(Store);
            AddSockets(m1, 2);
            var s = new Microsoft.Gadgeteer.Designer.SocketUse(Store);
            m1.SocketUses.Add(s);
            return m1;
        }
    }
}
