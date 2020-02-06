using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace xamarin_resources_generator {

    /**
     * MSBuild Task that generates a enum class named LocalizedString.cs,
     * that can be used from a .resx reader for fetch the resources
     */
    public class LocalizationEnumGenerator : Task {

        /**
         * Path to the source folder from which the task retrieves all the .resx files to be transformed
         */
        [Required]
        public string OriginFolder { get; set; }

        /**
         * Path to the folder where the task generates the LocalizedString enum class
         */
        [Required]
        public string DestinationFolder { get; set; }

        /**
         * package name to assign
         */
        [Required]
        public string PackageName { get; set; }

        public override bool Execute() {
            var originFolder = Directory.GetFiles(OriginFolder, "*.resx");

            foreach (string filePath in originFolder) {

                XDocument resxFile;
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                    resxFile = XDocument.Load(fs);
                }

                var classFilePath = Path.Combine(DestinationFolder, "LocalizedString.cs");

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
            }

            return true;
        }
    }
}
