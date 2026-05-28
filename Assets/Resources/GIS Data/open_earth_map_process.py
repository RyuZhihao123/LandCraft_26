from multiprocessing.util import is_exiting
import os
import numpy as np
from PIL import Image


DEFAULT_LABEL_COLOR_IMAGE_DIR = "color_label"

color_map = {
    1: (128, 0, 0),      # Bareland
    2: (0, 255, 36),     # Rangeland
    3: (148, 148, 148),  # Developed space
    4: (255, 255, 255),  # Road
    5: (34, 97, 38),     # Tree
    6: (0, 69, 255),     # Water
    7: (75, 181, 73),    # Agriculture land
    8: (222, 31, 7)      # Building
}


def get_label_color_image(path):
    if not os.path.exists(path):
        print("Path file not exist!")
        return

    label_image = Image.open(path)
    label_array = np.array(label_image)

    # unique, counts = np.unique(label_array, return_counts=True)
    # unique_counts = dict(zip(unique, counts))   

    # for value, count in unique_counts.items():
    #     print(f"value: {value}: Count: {count}")

    color_image_array = np.zeros((label_array.shape[0], label_array.shape[1], 3), dtype=np.uint8)

    for label, color in color_map.items():
        color_image_array[label_array == label] = color

    color_image = Image.fromarray(color_image_array)
    return color_image


def show_label_color_image(image_path):
    color_image = get_label_color_image(image_path)
    color_image.show()


def save_label_color_image(image_path):
    color_image = get_label_color_image(image_path)
    color_image.save(os.path.join(DEFAULT_LABEL_COLOR_IMAGE_DIR, "color_image.tif"))


def main():
    label_path = "label/baybay_26.tif"
    show_label_color_image(label_path)

if __name__ == "__main__":
    main()
