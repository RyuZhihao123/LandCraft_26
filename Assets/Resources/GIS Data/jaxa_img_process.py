import os
import cv2
import numpy as np
from PIL import Image


class JAXAImageProcess():
    def __init__(self, defualt_dir):
        self.default_dir = defualt_dir
        if not os.path.exists(self.default_dir):
            os.makedirs(self.default_dir)

        self.label_mapping = {
            (255, 0, 0): '_CITY',        # 人工构造物
            (0, 128, 255): '_CITY',      # 水田
            (255, 193, 191): '_CITY',    # 耕地
            (161, 41, 119): '_CITY',     # 太阳能板
            (255, 255, 255): '_CITY',    # 农业温室
            (255, 255, 0): '_FOREST',    # 草地
            (128, 255, 0): '_FOREST',    # 落叶阔叶林
            (0, 255, 128): '_FOREST',    # 落叶针叶林
            (86, 172, 0): '_FOREST',     # 常绿阔叶林
            (0, 172, 86): '_FOREST',     # 常绿针叶林
            (128, 100, 0): '_FOREST',    # 裸地
            (217, 240, 5): '_FOREST',    # 竹林
            (0, 150, 160): '_FOREST',    # 湿地
            (0, 0, 100): '_WATER',       # 水域
        }

        self.color_mapping = {
            '_CITY':    (0.5, 0.2, 0.2),
            '_FOREST':  (0.1, 0.8, 0.3),
            '_WATER':   (0.1, 0.1, 0.8)
        }

        self.color_mapping_255 = {
            k: tuple(int(v * 255) for v in color)
            for k, color in self.color_mapping.items()
        }

    def start_process_pipeline_from_begin(self, tif_img_path, crop_bounds, output_size=(600, 600)):
        ori_crop_img_path = os.path.join(self.default_dir, 'ori_crop_img.png')
        merge_label_img_path = os.path.join(self.default_dir, 'merge_label_img.png')
        denoise_img_path = os.path.join(self.default_dir, 'denoise_img.png')

        ori_crop_img = self.crop_and_resize(tif_image_path=tif_img_path,
                                            output_path=ori_crop_img_path,
                                            crop_bounds=crop_bounds,
                                            output_size=output_size)

        self.print_image_colors(ori_crop_img)

        self.real2synthesis(ori_crop_img, merge_label_img_path)

        self.denoise_in_synthesis_img(merge_label_img_path,
                                      denoise_img_path)

    # 裁剪
    def crop_and_resize(self, tif_image_path, output_path, crop_bounds, output_size):
        with Image.open(tif_image_path) as img:
            width, height = img.size

            left = crop_bounds[0][0] * width
            upper = crop_bounds[0][1] * height
            right = crop_bounds[1][0] * width
            lower = crop_bounds[1][1] * height

            cropped_img = img.crop((left, upper, right, lower))
            resized_img = cropped_img.resize(output_size, Image.LANCZOS)

            if resized_img.mode == 'P':
                resized_img = resized_img.convert('RGB')

            resized_img.save(output_path, format='PNG')

            return resized_img

    # 打印图片中所有颜色
    def print_image_colors(self, img):
        img = img.convert('RGB')
        colors = img.getcolors(maxcolors=50)

        if colors:
            for count, color in colors:
                print(f"Color: {color}, Count: {count}")
        else:
            print("Too many colors in the image or maxcolors is too small.")

    # 转为合成模式
    def real2synthesis(self, img, out_path):
        img_data = np.array(img)

        new_img_data = np.zeros((img_data.shape[0], img_data.shape[1], 3), dtype=np.float32)

        for original_color, synthetic_label in self.label_mapping.items():
            mask = np.all(img_data == np.array(original_color, dtype=np.uint8), axis=-1)
            new_color = self.color_mapping[synthetic_label]
            new_img_data[mask] = new_color

        new_img_data_uint8 = (new_img_data * 255).astype(np.uint8)
        new_img = Image.fromarray(new_img_data_uint8)
        new_img.save(out_path)

    # 去噪 
    def denoise_in_synthesis_img(self, input_path, output_path, kernel_size=8):
        img = cv2.imread(input_path)
        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)  # 确保图像格式为RGB

        kernel = np.ones((kernel_size, kernel_size), np.uint8)

        for landcover_type, color_val in self.color_mapping.items():
            color_val_255 = tuple(int(c * 255) for c in color_val)

            # 创建掩码
            mask = cv2.inRange(img, color_val_255, color_val_255)
            # 形态学开运算去掉小噪声
            opening = cv2.morphologyEx(mask, cv2.MORPH_OPEN, kernel)
            # 形态学闭运算填补小洞
            closing = cv2.morphologyEx(opening, cv2.MORPH_CLOSE, kernel)

            img[closing == 255] = color_val_255

        cleaned_img = Image.fromarray(img)
        cleaned_img.save(output_path)

def main():
    jaxa_img_process = JAXAImageProcess('N35E136')
    jaxa_img_process.start_process_pipeline_from_begin(
        'tif/LC_N35E136.tif',
        [[0.76316, 0.54211],[0.80526, 0.58421]],
    )

if __name__ == '__main__':
    main()
