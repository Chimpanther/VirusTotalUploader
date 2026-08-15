using System;
using System.Linq;
using System.Xml.Linq;

namespace UpdateProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var csprojPath = "../uploader/uploader.Tests/uploader.Tests.csproj";
            var doc = XDocument.Load(csprojPath);

            var itemGroups = doc.Descendants("ItemGroup").ToList();
            if (itemGroups.Count > 0)
            {
                var compileGroup = itemGroups.FirstOrDefault(ig => ig.Elements("Compile").Any());
                if (compileGroup != null)
                {
                    if (!compileGroup.Elements("Compile").Any(e => e.Attribute("Include")?.Value == "..\\uploader\\Settings.cs"))
                    {
                        compileGroup.Add(new XElement("Compile", new XAttribute("Include", "..\\uploader\\Settings.cs"), new XAttribute("Link", "Settings.cs")));
                    }
                    if (!compileGroup.Elements("Compile").Any(e => e.Attribute("Include")?.Value == "..\\uploader\\LocalizationHelper.cs"))
                    {
                        compileGroup.Add(new XElement("Compile", new XAttribute("Include", "..\\uploader\\LocalizationHelper.cs"), new XAttribute("Link", "LocalizationHelper.cs")));
                    }
                    if (!compileGroup.Elements("Compile").Any(e => e.Attribute("Include")?.Value == "..\\uploader\\LocalizationBase.cs"))
                    {
                        compileGroup.Add(new XElement("Compile", new XAttribute("Include", "..\\uploader\\LocalizationBase.cs"), new XAttribute("Link", "LocalizationBase.cs")));
                    }
                }
            }
            doc.Save(csprojPath);
            Console.WriteLine("Done.");
        }
    }
}
