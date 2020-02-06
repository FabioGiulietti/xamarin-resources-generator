using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace xamarin_resources_generator {

    /**
     * MsBuild task that transform all .resx files in a specified folder into
     * the corresponding android string format files.
     * for example:
     * resources.resx -> values/strings.xml
     * resources.fr.resx -> values-fr/strings.xml
     * resources.it.resx -> values-it/strings.xml
     */
    public class AndroidLanguagesGenerator : Task {

        /**
         * Path to the source folder from which the activity retrieves all the .resx files to be transformed
         */
        [Required]
        public string OriginFolder { get; set; }

        /**
         * Path to the target Android folder where the activity generates the values folders
         */
        [Required]
        public string DestinationFolder { get; set; }

        public override bool Execute() {
            var originFolder = Directory.GetFiles(OriginFolder, "*.resx");

            foreach (string filePath in originFolder) {

                XDocument resxFile;
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                    resxFile = XDocument.Load(fs);
                }

                var resources = new XElement("resources");
                XComment comm = new XComment("Do not update this file! If you would add or update a string see the .resx file in the common project!");
                resources.Add(comm);

                foreach (XElement str in resxFile.Root.Elements()) {
                    if (str.Name != "data")
                        continue;

                    string stringParsed = str.Value as string;
                    stringParsed = stringParsed.Replace("'", "\\'");
                    stringParsed = stringParsed.Replace("{newline}", @"\n");
                    stringParsed = stringParsed.Replace("{quotationMark}", @"\""");
                    stringParsed = stringParsed.Replace("&lt;", "<");
                    stringParsed = stringParsed.Replace("&gt;", ">");

                    resources.Add(new XElement("string", new XAttribute("name", str.FirstAttribute.Value), stringParsed));
                }

                XDocument result = new XDocument(resources);

                var fileName = Path.GetFileName(filePath).Split('.');
                var langPrefix = "";
                if (fileName.Length > 2)
                    langPrefix = "-" + fileName[1];

                var folder = Path.Combine(DestinationFolder, "values" + langPrefix);
                Directory.CreateDirectory(folder);
                result.Save(Path.Combine(folder, "strings.xml"));
            }

            return true;
        }
    }
}
