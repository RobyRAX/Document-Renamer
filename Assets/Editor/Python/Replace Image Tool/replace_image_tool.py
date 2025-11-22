import sys
import zipfile
from PIL import Image
import io
import shutil
import os

# ===============================
#   GENERIC UTILS
# ===============================

def load_zip_to_dict(zip_path):
    """Return {filename: bytes} dictionary"""
    with zipfile.ZipFile(zip_path, 'r') as zf:
        return {name: zf.read(name) for name in zf.namelist()}


def write_dict_to_zip(zip_path, file_buffer):
    """Rewrite zip with the given data"""
    with zipfile.ZipFile(zip_path, 'w') as zf:
        for name, data in file_buffer.items():
            zf.writestr(name, data)


def images_match(img_bytes, reference_path):
    """Pixel-level comparison"""
    img = Image.open(io.BytesIO(img_bytes)).convert("RGB")
    ref = Image.open(reference_path).convert("RGB")

    if img.size != ref.size:
        return False

    return list(img.getdata()) == list(ref.getdata())


# ===============================
#   DOCX HANDLER
# ===============================

DOCX_MEDIA = "word/media/"

def find_image_in_docx(path, reference_image_path):
    file_buffer = load_zip_to_dict(path)
    count = 0

    for name, data in file_buffer.items():
        if name.startswith(DOCX_MEDIA):
            if images_match(data, reference_image_path):
                count += 1

    return count


def replace_image_in_docx(path, reference_image_path, new_image_path):
    tmp = path + ".tmp"
    shutil.copyfile(path, tmp)

    file_buffer = load_zip_to_dict(tmp)

    new_img_bytes = open(new_image_path, 'rb').read()

    for name, data in file_buffer.items():
        if name.startswith(DOCX_MEDIA):
            if images_match(data, reference_image_path):
                file_buffer[name] = new_img_bytes

    write_dict_to_zip(tmp, file_buffer)
    shutil.move(tmp, path)


# ===============================
#   XLSX HANDLER
# ===============================

XLSX_MEDIA = "xl/media/"

def find_image_in_xlsx(path, reference_image_path):
    file_buffer = load_zip_to_dict(path)
    count = 0

    for name, data in file_buffer.items():
        if name.startswith(XLSX_MEDIA):
            if images_match(data, reference_image_path):
                count += 1

    return count


def replace_image_in_xlsx(path, reference_image_path, new_image_path):
    tmp = path + ".tmp"
    shutil.copyfile(path, tmp)

    file_buffer = load_zip_to_dict(tmp)

    new_img_bytes = open(new_image_path, 'rb').read()

    for name, data in file_buffer.items():
        if name.startswith(XLSX_MEDIA):
            if images_match(data, reference_image_path):
                file_buffer[name] = new_img_bytes

    write_dict_to_zip(tmp, file_buffer)
    shutil.move(tmp, path)


# ===============================
#   AUTO-DETECT HANDLER
# ===============================

def is_docx(path):
    return path.lower().endswith(".docx")

def is_xlsx(path):
    return path.lower().endswith(".xlsx")


def find_auto(path, ref_img):
    if is_docx(path):
        return find_image_in_docx(path, ref_img)
    elif is_xlsx(path):
        return find_image_in_xlsx(path, ref_img)
    else:
        print("ERROR: Unsupported file format")
        return 0


def replace_auto(path, ref_img, new_img):
    if is_docx(path):
        replace_image_in_docx(path, ref_img, new_img)
    elif is_xlsx(path):
        replace_image_in_xlsx(path, ref_img, new_img)
    else:
        print("ERROR: Unsupported file format")


# ===============================
#   CLI ENTRY
# ===============================

if __name__ == "__main__":
    mode = sys.argv[1].lower()

    if mode == "find":
        file = sys.argv[2]
        ref_img = sys.argv[3]
        print(find_auto(file, ref_img))

    elif mode == "replace":
        file = sys.argv[2]
        ref_img = sys.argv[3]
        new_img = sys.argv[4]
        replace_auto(file, ref_img, new_img)
