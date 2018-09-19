// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Microsoft.Gadgeteer.BuilderTasks
{
    public class XslTransformWithFunctions : Task
    {
        public enum ParameterMode
        {
            TaskItems,
            Contents
        }

        private class ItemsOrContentParameter
        {
            private readonly ParameterMode _mode;
            public ParameterMode Mode { get { return _mode; } }

            protected readonly string[] Data;
            public int Count { get { return Data.Length; } }

            public ItemsOrContentParameter(string itemsParameterName, string contentParameterName, ITaskItem[] items, params string[] contents)
            {
                if (items == null && contents[0] == null)
                    throw new ArgumentException(string.Format("One of {0} or {1} arguments must be set.", contentParameterName, itemsParameterName));

                if (items != null && contents[0] != null)
                    throw new ArgumentException(string.Format("Only one of {0} or {1} arguments can be set.", contentParameterName, itemsParameterName));

                if (items != null)
                {
                    Data = new string[items.Length];
                    
                    for (int i = 0; i < items.Length; i++)
                        Data[i] = items[i].ItemSpec;

                    _mode = ParameterMode.TaskItems;
                }
                else
                {
                    Data = contents;
                    _mode = ParameterMode.Contents;
                }
            }
        }
        private class XmlInput : ItemsOrContentParameter
        {
            public XmlInput(ITaskItem[] xmlFiles, string xml) : base("XmlInputPaths", "XmlContent", xmlFiles, xml) { }

            public XmlReader CreateReader(int index)
            {
                if (Mode == ParameterMode.TaskItems)
                    return XmlReader.Create(Data[index]);

                else
                    return XmlReader.Create(new StringReader(Data[index]));
            }
        }
        private class XslInput : ItemsOrContentParameter
        {
            public XslInput(ITaskItem[] xslFiles, string xsl) : base("XslInputPaths", "XslContent", xslFiles, xsl) { }

            public XslCompiledTransform CreateTransform(int index)
            {
                XslCompiledTransform transform = new XslCompiledTransform();

                if (Mode == ParameterMode.TaskItems)
                    transform.Load(new XPathDocument(Data[index]), XsltSettings.TrustedXslt, new XmlUrlResolver());

                else
                    transform.Load(XmlReader.Create(new StringReader(Data[index])));

                return transform;
            }
        }
        private class ParameterInput : ItemsOrContentParameter
        {
            public ParameterInput(ITaskItem[] parameterFiles, ITaskItem[] parameterContents)
                : base("ParameterPaths", "ParameterContents", parameterFiles, PrepareContents(parameterContents, parameterFiles == null)) { }

            private static string[] PrepareContents(ITaskItem[] parameterContents, bool allowDefault)
            {
                if (parameterContents == null)
                    return allowDefault ? new[] { "<parameters />" } : null;

                string[] contents = new string[parameterContents.Length];
                for (int i = 0; i < contents.Length; i++)
                    contents[i] = parameterContents[i].ItemSpec;

                return contents;
            }

            private XElement CreateXElement(int index)
            {
                if (Mode == ParameterMode.TaskItems)
                    return XElement.Load(Data[index]);

                else
                    return XElement.Parse(Data[index]);
            }

            public XsltArgumentList CreateArgumentList(int index)
            {
                XsltArgumentList arguments = new XsltArgumentList();
                arguments.AddExtensionObject("http://schemas.microsoft.com/Gadgeteer/2013/Tasks/ReflectionExtension", new ReflectionExtensionObject());

                XElement root = CreateXElement(index);
                foreach (XElement xParameter in root.Elements())
                    if (xParameter.Name.LocalName == "Parameter")
                    {
                        string name = (string)xParameter.Attribute("Name");
                        if (name == null)
                            throw new ArgumentException("The specified Xslt Parameter doesn't have attribute 'Name'.");

                        string value = (string)xParameter.Attribute("Value");
                        if (value == null)
                            throw new ArgumentException("The specified Xslt Parameter doesn't have attribute 'Value'.");

                        string ns = (string)xParameter.Attribute("Namespace");

                        arguments.AddParam(name, ns ?? string.Empty, value);
                    }

                return arguments;
            }
        }
        private class Output
        {
            private readonly string[] Data;
            public int Count { get { return Data == null ? 1 : Data.Length; } }

            public StringBuilder Content;

            public Output(ITaskItem[] outputPaths)
            {
                if (outputPaths != null)
                {
                    Data = new string[outputPaths.Length];
                    for (int i = 0; i < outputPaths.Length; i++)
                        Data[i] = outputPaths[i].ItemSpec;
                }

                Content = new StringBuilder();
            }

            public XmlWriter CreateWriter(int index, XmlWriterSettings settings)
            {
                if (Data == null)
                    return XmlWriter.Create(Content, settings);

                else
                    return XmlWriter.Create(Data[index], settings);
            }
        }

        public ITaskItem[] XmlInputPaths { get; set; }
        public string XmlContent { get; set; }

        public ITaskItem[] XslInputPaths { get; set; }
        public string XslContent { get; set; }

        public ITaskItem[] ParameterPaths { get; set; }
        public ITaskItem[] ParameterContents { get; set; }

        [Output] public ITaskItem[] OutputPaths { get; set; }
        [Output] public string OutputContent { get; set; }

        public override bool Execute()
        {
            XmlInput xmlInput = new XmlInput(XmlInputPaths, XmlContent);
            XslInput xslInput = new XslInput(XslInputPaths, XslContent);
            ParameterInput parameterInput = new ParameterInput(ParameterPaths, ParameterContents);
            Output output = new Output(OutputPaths);

            int maxCount = 0;

            maxCount = Math.Max(maxCount, xmlInput.Count);
            maxCount = Math.Max(maxCount, xslInput.Count);
            maxCount = Math.Max(maxCount, parameterInput.Count);
            maxCount = Math.Max(maxCount, output.Count);

            for (int i = 0; i < maxCount; i++)
            {
                XslCompiledTransform transform = xslInput.CreateTransform(Math.Min(xslInput.Count - 1, i));

                using (XmlWriter writer = output.CreateWriter(Math.Min(output.Count - 1, i), transform.OutputSettings))
                using (XmlReader reader = xmlInput.CreateReader(Math.Min(xmlInput.Count - 1, i)))
                    transform.Transform(reader, parameterInput.CreateArgumentList(Math.Min(parameterInput.Count - 1, i)), writer);
            }

            OutputContent = output.Content.ToString();
            return true;
        }

        private class ReflectionExtensionObject
        {
            public object Invoke(string type, string memberName)
            {
                return InvokeVariable(type, memberName, null);
            }
            public object Invoke1(string type, string memberName, object arg1)
            {
                return InvokeVariable(type, memberName, null, arg1);
            }
            public object Invoke2(string type, string memberName, object arg1, object arg2)
            {
                return InvokeVariable(type, memberName, null, arg1, arg2);
            }
            public object Invoke3(string type, string memberName, object arg1, object arg2, object arg3)
            {
                return InvokeVariable(type, memberName, null, arg1, arg2, arg3);
            }
            public object Invoke4(string type, string memberName, object arg1, object arg2, object arg3, object arg4)
            {
                return InvokeVariable(type, memberName, null, arg1, arg2, arg3, arg4);
            }
            public object Invoke5(string type, string memberName, object arg1, object arg2, object arg3, object arg4, object arg5)
            {
                return InvokeVariable(type, memberName, null, arg1, arg2, arg3, arg4, arg5);
            }
            public object Invoke6(string type, string memberName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
            {
                return InvokeVariable(type, memberName, null, arg1, arg2, arg3, arg4, arg5, arg6);
            }
            public object Invoke7(string type, string memberName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
            {
                return InvokeVariable(type, memberName, null, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
            public object Invoke8(string type, string memberName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
            {
                return InvokeVariable(type, memberName, null, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }

            public object InvokeVariable(string typeName, string memberName, params object[] args)
            {
                Type type = Type.GetType(typeName, true);

                return type.InvokeMember(memberName, BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.GetField, null, null, args);
            }
        }
    }
}
