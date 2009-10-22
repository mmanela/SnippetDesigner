using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Microsoft.SnippetDesigner
{
    /// <summary>
    ///    This is a custom attribute that shall be defined for all those properties of any class whose values need to be set based on 
    /// command line options. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AppCmdLineArgumentAttribute : Attribute
    {
        private readonly string name = "";
        private readonly string description = "";

        /// <summary>
        ///  Holds the name of the command line option that is associated with
        /// this property.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        ///  Holds the description of the property to which this is 
        /// associated.
        /// </summary>
        public string Description
        {
            get { return description; }
        }

        /// <summary>Attribute constructor.</summary>
        public AppCmdLineArgumentAttribute(string optionName, string descrip)
        {
            name = optionName;
            description = descrip;
        }
    }

    /// <summary>
    ///   A class that encapsulates parsing of command line options and provides
    /// support to update any class object that maps its properties to any one of 
    /// the command line option using custom attribute AppCmdLineArgumentAttribute .
    /// </summary>
    public class AppCmdLineArguments
    {
        private readonly StringDictionary Params;

        /// <summary>
        ///    Regular expressions to split each command line arguments into its parts.
        ///     ^-{1,2} this defines any argument which starts with "-" or "--" 
        ///     |^/| this defines any argument that starts with "/" 
        ///     |=|:| this defines the split delimiter could be = or : 
        ///   Thus the above regular expression could be used to split the following arguments samples into
        ///   key value pairs
        /// 
        ///     /plugin:"Name of the plugin" 
        ///     /trace=true
        ///     /debug:false
        /// 
        /// </summary>
        private readonly Regex splitter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        ///   This is used to trim the leading and trailing quotes.
        /// </summary>
        private readonly Regex trimQuotes = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        ///    Constructor that takes a array of command line arguments as a parameter
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public AppCmdLineArguments(string[] args)
        {
            Params = new StringDictionary();

            string param = null;
            string[] argument_parts;

            foreach (string str in args)
            {
                argument_parts = splitter.Split(str, 3);

                switch (argument_parts.Length)
                {
                        //. The argument has 3 parts like in example: /plugin:"Microsoft ASP.NET Analyzer"
                        //. In the above example argument_parts[1] will have plugin 
                        //. and argument_parts[2] will have the actual string "Microsoft ASP.NET Analyzer"
                    case 3:
                        if (param != null)
                        {
                            if (!Params.ContainsKey(param))
                                Params.Add(param, "true");
                        }
                        param = argument_parts[1];

                        if (!Params.ContainsKey(param))
                        {
                            Trim(ref argument_parts[2]);
                            Params.Add(param, argument_parts[2]);
                        }
                        param = null;
                        break;

                        //. The argument has only 2 parts like /trace
                        //. argument_parts[1] will be set to trace 
                    case 2:
                        if (param != null)
                        {
                            if (!Params.ContainsKey(param))
                                Params.Add(param, "true");
                        }
                        param = argument_parts[1];
                        break;

                        //. The argument has only one part esp in case of /trace true
                        //. In the above example argument_parts[0] would be true so the previous option's value is set to true
                        //. 
                    case 1:
                        if (param != null)
                        {
                            if (!Params.ContainsKey(param))
                            {
                                Trim(ref argument_parts[0]);
                                Params.Add(param, argument_parts[0]);
                            }
                        }
                        break;
                }
            }
            if (param != null)
            {
                if (!Params.ContainsKey(param))
                    Params.Add(param, "true");
            }
        }

        private void Trim(ref string Value)
        {
            Value = trimQuotes.Replace(Value, "$1");
        }

        /// <summary>
        ///    To support command line option value access like cmdLineArguments["plugin"]
        /// </summary>
        /// <param name="parameter">Command line option name</param>
        /// <returns>return its value</returns>
        public string this[string parameter]
        {
            get { return Params[parameter]; }
        }

        /// <summary>
        ///    Update the property value on the SnippetFile object passed whose AppCmdLineArgumentAttribute
        /// name matches the name of the option.
        /// </summary>
        /// <param name="SnippetFile">Any instance of a class</param>
        public void UpdateParams(object theApp)
        {
            if (theApp == null)
                return;

            PropertyInfo[] properties = theApp.GetType().GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                foreach (Attribute attribute in properties[i].GetCustomAttributes(false))
                {
                    if (attribute is AppCmdLineArgumentAttribute)
                    {
                        AppCmdLineArgumentAttribute argAttrib = attribute as AppCmdLineArgumentAttribute;
                        if (Params[argAttrib.Name] != null)
                        {
                            object[] propertyValue = new object[1];

                            if (properties[i].PropertyType == typeof (string))
                            {
                                propertyValue[0] = Params[argAttrib.Name];
                            }
                            else if (properties[i].PropertyType == typeof (int))
                            {
                                propertyValue[0] = int.Parse(Params[argAttrib.Name]);
                            }
                            else if (properties[i].PropertyType == typeof (bool))
                            {
                                propertyValue[0] = bool.Parse(Params[argAttrib.Name]);
                            }
                            properties[i].GetSetMethod().Invoke(theApp, propertyValue);
                        }
                    }
                }
            }
        }
    } ;
}