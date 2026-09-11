import xml.etree.ElementTree as ET
tree = ET.parse("uploader/uploader.Tests/uploader.Tests.csproj")
root = tree.getroot()

for item_group in root.findall("ItemGroup"):
    if item_group.find("Compile") is not None:
        compile_elem = ET.SubElement(item_group, "Compile")
        compile_elem.set("Include", "..\\uploader\\UploadForm.cs")
        compile_elem.set("Link", "UploadForm.cs")

        compile_elem2 = ET.SubElement(item_group, "Compile")
        compile_elem2.set("Include", "..\\uploader\\UploadForm.Designer.cs")
        compile_elem2.set("Link", "UploadForm.Designer.cs")

        compile_elem3 = ET.SubElement(item_group, "Compile")
        compile_elem3.set("Include", "..\\uploader\\MainForm.cs")
        compile_elem3.set("Link", "MainForm.cs")

tree.write("uploader/uploader.Tests/uploader.Tests.csproj")
