using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace XamarinResourcesGenerator {

    /**
     * MSBuild Task that generates a enum class named LocalizedString.cs,
     * that can be used from a .resx reader for fetch the resources
     */
    public class LocalizationEnumGenerator : Task {

        /**
         * Path to the source folder from which the task retrieves all the .resx files to be transformed
         * and in which enum and stringManager will be generated
         */
        [Required]
        public string OriginResxFolder { get; set; }

        /**
         * package name to assign to enum and stringManager
         */
        [Required]
        public string PackageName { get; set; }

        /**
         * AssemblyName from where the resx will be loaded
         */
        [Required]
        public string AssemblyName { get; set; }

        public override bool Execute() {
            var originFolder = Directory.GetFiles(OriginResxFolder, "StringResources.resx");

            foreach (string filePath in originFolder) {

                XDocument resxFile;
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                    resxFile = XDocument.Load(fs);
                }

                var classFilePath = Path.Combine(OriginResxFolder, "LocalizedString.cs");

                //clear class
                File.WriteAllText(classFilePath, string.Empty);

                using (StreamWriter file = new StreamWriter(classFilePath, true)) {
                    file.WriteLine("namespace " + PackageName + " {");
                    file.WriteLine("//Do not update this file! If you would add or update a string see the .resx file in the common project!");
                    file.WriteLine("public enum LocalizedString {");

                    foreach (XElement str in resxFile.Root.Elements()) {
                        if (str.Name != "data")
                            continue;

                        string name = str.FirstAttribute.Value;
                        file.WriteLine(name + ",");
                    }

                    file.WriteLine("}");
                    file.WriteLine("}");
                }

                classFilePath = Path.Combine(OriginResxFolder, "StringManager.cs");

                //clear class
                File.WriteAllText(classFilePath, string.Empty);
                
                using (StreamWriter file = new StreamWriter(classFilePath, true)) {
                    var classToWrite = GetStringManagerClassText();
                    file.Write(classToWrite);
                }
            }

            return true;
        }

        private string GetStringManagerClassText() {
            return @"
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace " + PackageName + @" {

    /*
     * Do not update this file! If you would add or update a string see the .resx file in the common project!
     */
    public class StringManager {

        private static StringManager instance;

        private static readonly string AssemblyName = """ + AssemblyName + @""";

        private static readonly string NamespaceResx = """ + PackageName + @".StringResources"";

        private CultureInfo CultureInfo;

        private readonly Dictionary<LocalizedString, string> dynamicStrings = new Dictionary<LocalizedString, string>();

        public static StringManager Instance {
            get {
                if (instance == null) {
                    instance = new StringManager();
                }
                return instance;
            }
        }

        readonly ResourceManager resourceManager;

        private StringManager() {
            SetCulture(CultureInfo.CurrentCulture);
            var ass = new AssemblyName(AssemblyName);
            resourceManager = new ResourceManager(NamespaceResx, Assembly.Load(ass));
        }

        public string GetString(LocalizedString stringEnum) {
            if (dynamicStrings.ContainsKey(stringEnum))
                return dynamicStrings[stringEnum];

            string text = null;
            if (CultureInfo != null)
                text = resourceManager.GetString(stringEnum.ToString(), CultureInfo);

            if (string.IsNullOrEmpty(text))
                text = resourceManager.GetString(stringEnum.ToString());

            return text
                .Replace(""{newline}"", Environment.NewLine)
                .Replace(""{quotationMark}"", ""\u0022"");
        }

        public string GetString(LocalizedString stringEnum, CultureInfo culture) {
            return resourceManager.GetString(stringEnum.ToString(), culture);
        }

        public void PutString(LocalizedString stringEnum, string value) {
            if (dynamicStrings.ContainsKey(stringEnum))
                dynamicStrings[stringEnum] = value;
            else
                dynamicStrings.Add(stringEnum, value);
        }

        public void SetCulture(CultureInfo cultureInfo) {
            this.CultureInfo = cultureInfo;
        }
    }
}";
        }
    }
}
