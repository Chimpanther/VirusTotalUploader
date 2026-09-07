import xml.etree.ElementTree as ET
import os

tree = ET.parse('uploader/uploader/uploader.csproj')
root = tree.getroot()

ns = {'ns': 'http://schemas.microsoft.com/developer/msbuild/2003'}
ET.register_namespace('', 'http://schemas.microsoft.com/developer/msbuild/2003')

item_groups = root.findall('ns:ItemGroup', ns)
compile_group = None

for group in item_groups:
    compiles = group.findall('ns:Compile', ns)
    if compiles:
        compile_group = group
        break

if compile_group is not None:
    found = False
    for compile in compile_group.findall('ns:Compile', ns):
        if compile.get('Include') == 'SettingsPresenter.cs':
            found = True
            break

    if not found:
        new_compile = ET.Element('{http://schemas.microsoft.com/developer/msbuild/2003}Compile')
        new_compile.set('Include', 'SettingsPresenter.cs')
        compile_group.append(new_compile)

        with open('uploader/uploader/uploader.csproj', 'wb') as f:
            f.write(b'<?xml version="1.0" encoding="utf-8"?>\r\n')
            tree.write(f, encoding='utf-8', xml_declaration=False)
        print("uploader.csproj updated.")
else:
    print("Could not find Compile ItemGroup")
