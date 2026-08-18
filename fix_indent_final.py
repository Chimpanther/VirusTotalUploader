import sys

def main():
    file_path = "uploader/uploader/Utils.cs"
    with open(file_path, "rb") as f:
        content = f.read().decode("utf-8-sig")

    content = content.replace("                    var checksum", "                    var checksum")

    with open(file_path, "wb") as f:
        f.write(b'\xef\xbb\xbf')
        f.write(content.encode("utf-8"))

if __name__ == "__main__":
    main()
