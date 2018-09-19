// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Modeling;
using Microsoft.Gadgeteer.Designer;
using System.Collections.ObjectModel;
using Microsoft.Gadgeteer.Designer.Definitions;

namespace GadgeteerDesignerUnitTests
{
	[TestClass]
	public class AutoConnectTest
	{

        [TestMethod]
        public void TestSolve()
        {
            var ps = new Collection<ProvidedSocket>();
            ps.Add(new ProvidedSocket() { Types = new Collection<string>() { "A" }, Label = "ps1" });
            ps.Add(new ProvidedSocket() { Types = new Collection<string>() { "B" }, Label = "ps2" });
            ps.Add(new ProvidedSocket() { Types = new Collection<string>() { "C" }, Label = "ps3" });
			ps.Add(new ProvidedSocket() { Types = new Collection<string>() { "D" }, Label = "ps4" });
			ps.Add(new ProvidedSocket() { Types = new Collection<string>() { "O" }, Label = "ps5" });

            var mbDefs = new List<MainboardDefinition>() {
				ModelTest.CreateMainboardDef("happy", ps)
			};

            var moDefs = new List<ModuleDefinition>() {
				ModelTest.CreateModuleDef(Guid.Parse("015DF700-5A47-4282-BD91-BFD024D038FC"),"m1su1","A",false),
				ModelTest.CreateModuleDef(Guid.Parse("9AB4B04B-A31B-4E8C-82E6-532456B8C0A0"),"m2su1","B,C",false),
				ModelTest.CreateModuleDef(Guid.Parse("3B5F45A2-EE5A-4FE7-83E6-900D7096F0E2"),"m3su1","B,A",false),
				ModelTest.CreateModuleDef(Guid.Parse("5FE253D2-3B61-4F84-8F4B-8B9B05AEE0AE"),"m4su1","D",false),
				//ModelTest.CreateModuleDef(Guid.Parse("A5C823F5-88B8-4F33-BE2F-9D1956C9ED9C"),"m5su1","D",true),
				//ModelTest.CreateModuleDef(Guid.Parse("3FF4E16B-2DAD-4B93-BCF3-5FE359F95079"),"m6su1","O",true),
			};


            GadgeteerDefinitionsManager.Instance.Initialize(mbDefs, moDefs);

            var store = new Store(typeof(GadgeteerDSLDomainModel));
            var tx = store.TransactionManager.BeginTransaction();

            var mainboard = new Mainboard(store);
            mainboard.Name = "happy";
            mainboard.CreateSockets();

            var modules = new Collection<Module>();

            var ids = new Collection<Guid>();
            ids.Add(Guid.Parse("015DF700-5A47-4282-BD91-BFD024D038FC"));
            ids.Add(Guid.Parse("9AB4B04B-A31B-4E8C-82E6-532456B8C0A0"));
            ids.Add(Guid.Parse("3B5F45A2-EE5A-4FE7-83E6-900D7096F0E2"));
			ids.Add(Guid.Parse("5FE253D2-3B61-4F84-8F4B-8B9B05AEE0AE"));
			//ids.Add(Guid.Parse("A5C823F5-88B8-4F33-BE2F-9D1956C9ED9C"));
			//ids.Add(Guid.Parse("3FF4E16B-2DAD-4B93-BCF3-5FE359F95079"));

            foreach (var guid in ids)
            {
                var m = new Module(store) { ModuleDefinitionId = guid };
                m.CreateSocketUses();
                modules.Add(m);
            }


            //Basic test where all works
            using (var tx2 = store.TransactionManager.BeginTransaction())
            {
				var socketUses = from o in modules from su in o.SocketUses select su;
				var b1 = AutoConnect.Solve(mainboard, socketUses);
				Assert.IsTrue(b1, "Complex connection");
                tx2.Rollback();
            }

			// Test where there are mutiple optional sockets with the same type
			/*
			using (var tx2 = store.TransactionManager.BeginTransaction())
			{
				(GadgeteerDefinitionsManager.Instance.Modules.Last().Sockets[0].Types[0] as SingleSocketType).Value = "D";
				var socketUses = from o in modules from su in o.SocketUses select su;
				var b1 = AutoConnect.Solve(mainboard, socketUses);
				Assert.IsTrue(b1, "Multiple optional sockets with the same type");
				Assert.IsNotNull(modules[3].SocketUses[0].Socket);
				tx2.Rollback();
			}
			 */

			// Test where an optional socket cannot connect
			/*
			using (var tx2 = store.TransactionManager.BeginTransaction())
			{
				(GadgeteerDefinitionsManager.Instance.Modules.Last().Sockets[0].Types[0] as SingleSocketType).Value = "Z";
				var socketUses = from o in modules from su in o.SocketUses select su;
				var b1 = AutoConnect.Solve(mainboard, socketUses);
				Assert.IsTrue(b1, "Optional socket cannot connect");
				tx2.Rollback();
			}
			*/

			//Start from a partially connected state
            using (var tx2 = store.TransactionManager.BeginTransaction())
            {
                modules[0].SocketUses[0].Socket = mainboard.Sockets[0];
				var socketUses = from o in modules from su in o.SocketUses select su;
				var b1 = AutoConnect.Solve(mainboard, socketUses);
				Assert.IsTrue(b1, "Partial connection");
                tx2.Rollback();
            }

            using (var tx2 = store.TransactionManager.BeginTransaction())
            {
                (GadgeteerDefinitionsManager.Instance.Modules.First().Sockets[0].Types[0] as SingleSocketType).Value = "E";
				var socketUses = from o in modules from su in o.SocketUses select su;
				var b1 = AutoConnect.Solve(mainboard, socketUses);
				Assert.IsFalse(b1, "A module that cannot connect to the mainboard");
                tx2.Rollback();
            }


            using (var tx2 = store.TransactionManager.BeginTransaction())
            {
                (GadgeteerDefinitionsManager.Instance.Modules.First().Sockets[0].Types[0] as SingleSocketType).Value = "A";
                GadgeteerDefinitionsManager.Instance.Mainboards.First().ProvidedSockets.RemoveAt(3);
                mainboard.CreateSockets();
				var socketUses = from o in modules from su in o.SocketUses select su;
				var b1 = AutoConnect.Solve(mainboard, socketUses);
				Assert.IsFalse(b1, "Not enough sockets on the mainboard");
                tx2.Rollback();
            }

            tx.Rollback();
            store.Dispose();
        }
	}
}
