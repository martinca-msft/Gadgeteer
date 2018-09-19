// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Gadgeteer.Designer.Definitions;
using Microsoft.Gadgeteer.Designer.Resources;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;
 

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Rule used to add the Sockets to a board
    /// </summary>
    [RuleOn(typeof(Mainboard), FireTime = TimeToFire.TopLevelCommit)]
    class MainboardAddedRule : GadgeteerAddRule<Mainboard>
    {
        public MainboardAddedRule() : base(false) { }              

        public override void ElementAdded(Mainboard board)
        {
            Store store = board.Store;            
            board.CacheDefinition();

			if (board.Sockets.Count == 0)
                board.CreateSockets();

            if (string.IsNullOrWhiteSpace(board.Name))
            {
                var boardDef = GadgeteerDefinitionsManager.Instance.Mainboards.FirstOrDefault();
                if (boardDef != null)
                    board.Name = boardDef.Name;
            }

            if (!VerifyMinimumGadgeteerVersion(board, store))
                return;

            ReplaceExistingMainboard(board, store);

        }

        private void ReplaceExistingMainboard(Mainboard board, Store store)
        {
            //Don't check if this is the toolbox loading code executing or the code generation. None of this execute in the context of a project.            
            if (store.PropertyBag.ContainsKey(GadgeteerModel.DesignerStoreCookie) &&
                store.ElementDirectory.FindElements<Mainboard>().Count > 1)
            {
                DialogResult result = MessageBox.Show(Resources.UI.ReplaceMainboardPrompt,
                                                      Resources.UI.MessageBoxTitle,
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var mainboards = store.ElementDirectory.FindElements<Mainboard>().Where(mb => mb != board).ToArray();
                    foreach (var mb in mainboards)
                    {
                        board.GadgeteerModel.GadgeteerHardware.Remove(mb);
                        mb.Delete();
                    }
                }
                else
                {
                    Fail(null, store);
                }
            }
        }
    }
   
    /// <summary>
    /// Rule used to ensure we always have 1 Mainboard present
    /// </summary>
    [RuleOn(typeof(GadgeteerModel), FireTime = TimeToFire.TopLevelCommit)]
    class GadgeteerModelAddRule : GadgeteerAddRule<GadgeteerModel>
    {        

        public GadgeteerModelAddRule() : base(false) { }

        public override void ElementAdded(GadgeteerModel model)
        {
            if (!model.NewModel)
                return;

            //Have diagrams start with the mainboard and comment with usage tips

            if (model.GadgeteerHardware.Count(h => h is Mainboard)==0)
            {
                object oVersion;
                if (model.Store.PropertyBag.TryGetValue(GadgeteerModel.ProjectMicroframeworkVersionKey, out oVersion))
                {
                    string netMFVersion = oVersion as string;
                    var mainboard = GetDefaultMainboard(model.Store, netMFVersion as string);
                    if (mainboard != null)
                        model.GadgeteerHardware.Add(mainboard);
                }
            }

            if (model.Comments.Count==0)
            {
                var comment = new Comment(model.Store);
                comment.Text = Resources.UI.StartTip;
                model.Comments.Add(comment);
            }

            model.NewModel = false;
        }

        private Mainboard GetDefaultMainboard(Store store, string netMFVersion)
        {
            if (string.IsNullOrWhiteSpace(netMFVersion))
                return null;

            MainboardDefinition definition = GadgeteerModel.GetLastUsedMainboardDefinition(netMFVersion);

            if(definition==null)
                definition = GadgeteerDefinitionsManager.Instance.Mainboards.Where(d => d.SupportedMicroframeworkVersions.Contains(netMFVersion)).FirstOrDefault();

            if (definition == null)
                return null;


            return new Mainboard(store, true /*Flag this was created as part of a new model */) 
                                  { Name = definition.Name };
        }
        
    }

    /// <summary>
    /// Rule used to create sockets and cache the module definition
    /// </summary>
    [RuleOn(typeof(Module), FireTime = TimeToFire.TopLevelCommit)]
    class ModuleAddRule : GadgeteerAddRule<Module>
    {
        public ModuleAddRule() : base(false) { }

        public override void ElementAdded(Module module)
        {
            if (!VerifyMinimumGadgeteerVersion(module, module.Store))
                return;

            IEnumerable<string> errors = module.GetMissingDependencyErrors();
            if (errors.Count() > 0)
            {
                Fail(string.Format(UI.ModuleCouldNotBeAdded, string.Join(Environment.NewLine+"- ", (new []{""}).Concat(errors))), module.Store);
                return;
            }

            if(module.Sockets.Count==0)
                module.CreateSockets();

            if(module.SocketUses.Count==0)
                module.CreateSocketUses();

            module.CacheDefinition();
        }
    }

    /// <summary>
    /// If the Name of the Mainboard changes, re-create the sockets ...
    /// </summary>
    [RuleOn(typeof(Mainboard), FireTime = TimeToFire.TopLevelCommit)]
    class MainboardNameChangedRule : ChangeRule
    {        
        public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
        {
            base.ElementPropertyChanged(e);            

            if (e.DomainProperty.Id == Mainboard.NameDomainPropertyId)
            {
                var mb = e.ModelElement as Mainboard;
                if (mb != null && !string.IsNullOrWhiteSpace(e.OldValue as string))
                    mb.CreateSockets();

                var shape = PresentationViewsSubject.GetPresentation(mb).FirstOrDefault() as GadgeteerHardwareShape;
                if(shape!=null)
                    shape.OnDefinitionChanged();                
            }
        }
    }

    /// <summary>
    /// Simple base class to factor out the casting of the added element
    /// and the serialization check
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class GadgeteerAddRule<T> : AddRule where T : ModelElement
    {
        private readonly bool skipIfSerializing = true;
        RuleHelper helper = new RuleHelper();

        protected GadgeteerAddRule() { }

        protected GadgeteerAddRule(bool skipIfSerializing) 
        {
            this.skipIfSerializing = skipIfSerializing;
        }

        public override void ElementAdded(ElementAddedEventArgs e)
        {
            base.ElementAdded(e);
            T element = e.ModelElement as T;
            if (element != null)
            {
                if (skipIfSerializing && element.Store.TransactionManager.CurrentTransaction.IsSerializing)
                    return;

                ElementAdded(element);
            }
        }

        public abstract void ElementAdded(T element);
        

        protected bool VerifyMinimumGadgeteerVersion(GadgeteerHardware hardware, Store store)
        {
            //Don't check if this is the toolbox loading code executing or the code generation. None of this execute in the context of a project.
            if (!store.PropertyBag.ContainsKey(GadgeteerModel.DesignerStoreCookie))
                return true;

            bool result = CheckGadgeteerMinimumVersion(hardware, store);
            if (!result)
            {
                Fail(string.Format(Resources.UI.MinimumGadgeteerVersionNotMet, hardware.GadgeteerPartDefinition.Name,
                                                                  hardware.GetType().Name.ToLowerInvariant(),
                                                                  hardware.GadgeteerPartDefinition.MinimumGadgeteerCoreVersion), store);                                                                  

            }
            return result;

        }

        private static bool CheckGadgeteerMinimumVersion(GadgeteerHardware hardware, Store store)
        {
            object gadgeteerVersion = null;
            bool result = false;
            if (store.PropertyBag.TryGetValue(GadgeteerModel.GadgeteerCoreVersionKey, out gadgeteerVersion))
            {
                if (hardware.GadgeteerPartDefinition == null || string.IsNullOrWhiteSpace(hardware.GadgeteerPartDefinition.MinimumGadgeteerCoreVersion))
                {
                    Log.WriteError("Can't find the MinimumGadgeteerCoreVersion for " + hardware);
                    result = true; //Nothing to compare to. Not good but make sure we don't block all modules
                }
                else
                {
                    var version = (Version)gadgeteerVersion;
                    result = (new Version(hardware.GadgeteerPartDefinition.MinimumGadgeteerCoreVersion)) <= version;
                }
            }
            else
            {
                Log.WriteError("We don't know the version Gadgeteer.dll being referenced");
            }
            return result;
        }
        
        protected void Fail(string message, Store store)
        {
            helper.Fail(message, store);
        }
    }

    [RuleOn(typeof(GadgeteerModelHasGadgeteerHardware), FireTime = TimeToFire.TopLevelCommit)]
    class GadgeteerHardwareRemovedRule : DeleteRule
    {
        RuleHelper helper = new RuleHelper();
        public override void ElementDeleted(ElementDeletedEventArgs e)
        {
            base.ElementDeleted(e);
            var link = e.ModelElement as GadgeteerModelHasGadgeteerHardware;
            if(link==null)
                return;

            foreach (Module m in link.GadgeteerModel.Modules)
            {
                if (m.GetMissingDependencyErrors().Count() > 0)
                {
                    helper.Fail(string.Format(UI.BrokenDependencies, m.Name), m.Store);
                    return;
                }
            }
        }
    }

    class RuleHelper
    {
        private CommitBlocker commitBlocker;
        public CommitBlocker GetCommitBlocker(Store store)
        {
            if (commitBlocker == null)
                commitBlocker = new CommitBlocker(store);
            return commitBlocker;
        }

        public void Fail(string message, Store store)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show(message,
                                Resources.UI.MessageBoxTitle,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }

            GetCommitBlocker(store).BlockCommitIf(true);
        }
    }

}
