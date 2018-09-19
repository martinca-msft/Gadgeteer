// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Gadgeteer.Designer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.Modeling;
using System.Collections.Generic;
using System.Linq;

namespace GadgeteerDesignerUnitTests
{
    
    
    /// <summary>
    ///This is a test class for GadgeteerModelTest and is intended
    ///to contain all GadgeteerModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GadgeteerModelTest : ModelTest
    {
        /// <summary>
        ///A test for GetNamespaces
        ///</summary>
        [TestMethod()]
        public void GetNamespacesTest()
        {
            GadgeteerModel target = null;
            DoInTransaction(() =>
                {
                    target = this.GadgeteerModel;
                    AddModuleOfType(target, "Foo"); //no namespace
                    AddModuleOfType(target, ""); //empty or missing typename, make sure we don't crash
                    AddModuleOfType(target, "A.B.C"); 
                    AddModuleOfType(target, "Bar.Foo"); //empty or missing typename, make sure we don't crash
                    AddModuleOfType(target, "A.B.C");  //Ensure no duplicates
                });
            
            Assert.IsTrue(target.GetModuleNamespaces().SequenceEqual(new []{"A.B", "Bar"}));
        }

        private void AddModuleOfType(GadgeteerModel target, string moduleType)
        {
            Module m = new Module(Store);
            m.ModuleType = moduleType;
            target.GadgeteerHardware.Add(m);                
        }

        /// <summary>
        ///A test for GetNamespace
        ///</summary>
        [TestMethod()]
        public void GetNamespaceTest()
        {
            Action<string,string> test = (typeName, expectedNamespace) =>
                {
                    string actual = GadgeteerModel.GetNamespace(typeName);
                    Assert.AreEqual(expectedNamespace, actual);
                };

            test("", null);
            test("Foo", null);
            test("Bar.Foo", "Bar");
            test("A.Bar.Foo", "A.Bar");
        }

        [TestMethod]
        public void TestInitialMainboardCreation()
        {
            //For new models, a mainboard should be created, but not for existing models
            TestModelCreation(true, 1);
            TestModelCreation(false, 0);
        }

        private static void TestModelCreation(bool newModel, int expectedMainboards)
        {
            var store = new Store(typeof(GadgeteerDSLDomainModel));

            using (var tx = store.TransactionManager.BeginTransaction())
            {
                var model = new GadgeteerModel(store);
                model.NewModel = newModel;
                tx.Commit();
            }
            int mbCount = store.ElementDirectory.FindElements<Mainboard>().Count;
            Assert.AreEqual(expectedMainboards, mbCount);
        }
    }
}
