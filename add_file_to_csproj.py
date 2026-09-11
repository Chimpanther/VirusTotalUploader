import xml.etree.ElementTree as ET
tree = ET.parse("uploader/uploader.Tests/uploader.Tests.csproj")
root = tree.getroot()

# Find itemgroup with compiles
for item_group in root.findall("ItemGroup"):
    if item_group.find("Compile") is not None:
        compile_elem = ET.SubElement(item_group, "Compile")
        compile_elem.set("Include", "..\\uploader\\VirusTotalClient.cs")
        compile_elem.set("Link", "VirusTotalClient.cs")

tree.write("uploader/uploader.Tests/uploader.Tests.csproj")
