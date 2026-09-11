import re

with open('uploader/uploader.Tests/uploader.Tests.csproj', 'r', encoding='utf-8') as f:
    content = f.read()

# Add SettingsFormHelpers.cs to ItemGroup
if 'SettingsFormHelpers.cs' not in content:
    content = content.replace(
        '<Compile Include="..\\uploader\\LocalizationHelper.cs" />',
        '<Compile Include="..\\uploader\\LocalizationHelper.cs" />\n    <Compile Include="..\\uploader\\SettingsFormHelpers.cs" />'
    )
    with open('uploader/uploader.Tests/uploader.Tests.csproj', 'w', encoding='utf-8') as f:
        f.write(content)
