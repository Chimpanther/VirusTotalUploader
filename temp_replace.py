import sys

def replace_in_file(filepath, search, replace):
    with open(filepath, 'rb') as f:
        content = f.read()

    content = content.replace(search.encode('utf-8'), replace.encode('utf-8'))

    with open(filepath, 'wb') as f:
        f.write(content)
