// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Gadgeteer.Designer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.Modeling;
using Microsoft.Gadgeteer.Designer.Definitions;
using System.Collections.ObjectModel;
using System.Collections.Generic;


namespace GadgeteerDesignerUnitTests
{


	/// <summary>
	///This is a test class for SocketUseTest and is intended
	///to contain all SocketUseTest Unit Tests
	///</summary>
	[TestClass()]
	public class SocketUseTest
	{


		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

		[TestMethod()]
		public void PinConnectTest()
		{
			var providedSockets = new Collection<ProvidedSocket>() {
				new ProvidedSocket() { Label="mb1ps1", SharedPinMaps= new Collection<SharedPinMap>() {
					new SharedPinMap() {NetId = "spm1", SocketPin = 1 },
					new SharedPinMap() {NetId = "spm2", SocketPin = 2 }
				}},
				new ProvidedSocket() { Label="mb1ps2", SharedPinMaps= new Collection<SharedPinMap>() { 
					new SharedPinMap() { NetId = "spm1", SocketPin = 3 } }},
				new ProvidedSocket() { Label="mb1ps3"},
				new ProvidedSocket() { Label="mb1ps4", SharedPinMaps= new Collection<SharedPinMap>() {
					new SharedPinMap() {NetId = "spm1", SocketPin = 2 },
					new SharedPinMap() {NetId = "spm2", SocketPin = 4 }
				}}
			};

			var mbDefs = new List<MainboardDefinition>() {
				ModelTest.CreateMainboardDef("happy", providedSockets)
			};

			var moduleGuid = Guid.Parse("015DF700-5A47-4282-BD91-BFD024D038FC");


			var moDefs = new List<ModuleDefinition>() {
				ModelTest.CreateModuleDef(moduleGuid, new Collection<Microsoft.Gadgeteer.Designer.Definitions.SocketUse>() {
					ModelTest.CreateSocketUseDef("m1su1", new List<string>() {"A"}, null),
					ModelTest.CreateSocketUseDef("m1su2", new List<string>() {"A"}, null),
					ModelTest.CreateSocketUseDef("m1su3", new List<string>() {"A"}, null),
					ModelTest.CreateSocketUseDef("m1su4", new List<string>() {"A"}, null)
				})
			};

			//This connects to provided socket 0, which has the shared pin on socket pin 1
			moDefs[0].Sockets[0].Pins = new Collection<Pin>() {
				new Pin() { Shared = true, Value=1}, //This pin uses the mainboard shared pin
				new Pin() { Shared = true, Value=2},
				new Pin() { Shared = false, Value=3},
				new Pin() { Shared = false, Value=4}
			};

			//These next three connect to other provided sockets
			moDefs[0].Sockets[1].Pins = new Collection<Pin>() {
				new Pin() { Shared = false, Value=1},
				new Pin() { Shared = true, Value=2},
				//new Pin() { Shared = false, Value=3},	 //Don't use 3 to test a socket that doesn't use the shared pin
				new Pin() { Shared = false, Value=4}
			};

			moDefs[0].Sockets[2].Pins = new Collection<Pin>() {
				new Pin() { Shared = false, Value=1},
				new Pin() { Shared = true, Value=2},
				new Pin() { Shared = false, Value=3},
				new Pin() { Shared = true, Value=4}
			};

			moDefs[0].Sockets[3].Pins = new Collection<Pin>() {
				new Pin() { Shared = false, Value=1},
				new Pin() { Shared = false, Value=2},
				new Pin() { Shared = true,  Value=3},
				new Pin() { Shared = false, Value=4}
			};
			GadgeteerDefinitionsManager.Instance.Initialize(mbDefs, moDefs);

			var store = new Store(typeof(GadgeteerDSLDomainModel));
			var tx = store.TransactionManager.BeginTransaction();

			var mainboard = new Mainboard(store);
			mainboard.Name = "happy";
			mainboard.CreateSockets();

			var module = new Module(store);
			module.ModuleDefinitionId = moduleGuid;
			module.CreateSocketUses();

			Func<bool> testConnect = () => Microsoft.Gadgeteer.Designer.SocketUse.CanPinsConnect(mainboard.Sockets[0], module.SocketUses[0]);

			Assert.IsTrue(testConnect(), "No other sockets connected");

			using (var tx2 = store.TransactionManager.BeginTransaction())
			{
				mainboard.Sockets[2].SocketUse = module.SocketUses[1];
				Assert.IsTrue(testConnect(), "Sockets connected but no pin conflict");
				tx2.Rollback();
			}

			using (var tx2 = store.TransactionManager.BeginTransaction())
			{
				mainboard.Sockets[1].SocketUse = module.SocketUses[2];
				Assert.IsFalse(testConnect(), "Sockets connected and a pin conflict");
				tx2.Rollback();
			}

			using (var tx2 = store.TransactionManager.BeginTransaction())
			{
				mainboard.Sockets[1].SocketUse = module.SocketUses[3];
				Assert.IsTrue(testConnect(), "Sockets connected and a pin conflict but is shareable");
				tx2.Rollback();
			}

			using (var tx2 = store.TransactionManager.BeginTransaction())
			{
				mainboard.Sockets[3].SocketUse = module.SocketUses[1];
				Assert.IsFalse(testConnect(), "Sockets connected and pins conflict but are not shareable");
				tx2.Rollback();
			}

			using (var tx2 = store.TransactionManager.BeginTransaction())
			{
				mainboard.Sockets[3].SocketUse = module.SocketUses[2];
				Assert.IsTrue(testConnect(), "Sockets connected and pins conflict but and are shareable");
				tx2.Rollback();
			}



			tx.Rollback();
			tx.Dispose();
			store.Dispose();







			//var whee = Microsoft.Gadgeteer.Designer.SocketUse.PinConnect(providedSockets[0], socketUse, providedSockets);

		}


		[TestMethod()]
		public void CanConnectTest()
		{
			var providedSocket = new ProvidedSocket();
			var socketUse = new Microsoft.Gadgeteer.Designer.Definitions.SocketUse();

			Assert.IsFalse(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "null collections");

			socketUse.Types = new Collection<SocketType>();
			socketUse.Types.Add(new SingleSocketType() { Value = "A" });
			socketUse.Types.Add(new SingleSocketType() { Value = "B" });
			var composite = new CompositeType();
			composite.Types = new Collection<SingleSocketType>();
			composite.Types.Add(new SingleSocketType() { Value = "Z" });
			composite.Types.Add(new SingleSocketType() { Value = "Y" });
			socketUse.Types.Add(composite);

			Assert.IsFalse(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "Tried to connect nothing");
			providedSocket.Types = new Collection<string>();
			Assert.IsFalse(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "Empty collection");

			providedSocket.Types.Add("A");
			Assert.IsTrue(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "Connect to 'A'");

			providedSocket.Types.Clear();
			providedSocket.Types.Add("a");
			Assert.IsFalse(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "Connect to 'a'");

			providedSocket.Types.Clear();
			providedSocket.Types.Add("C");
			Assert.IsFalse(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "Tried to connect to 'C'");

			providedSocket.Types.Clear();
			providedSocket.Types.Add("B");
			providedSocket.Types.Add("C");
			Assert.IsTrue(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "Connect to 'B' or 'C'");

			providedSocket.Types.Clear();
			providedSocket.Types.Add("B");
			providedSocket.Types.Add("Z");
			Assert.IsTrue(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "Connect to single with part of composite");

			providedSocket.Types.Clear();
			providedSocket.Types.Add("Z");
			Assert.IsFalse(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "try to connect to composite");

			providedSocket.Types.Clear();
			providedSocket.Types.Add("Y");
			providedSocket.Types.Add("Z");
			Assert.IsTrue(Microsoft.Gadgeteer.Designer.SocketUse.CanTypeConnect(providedSocket, socketUse), "Connect to composite");

		}
	}
}
