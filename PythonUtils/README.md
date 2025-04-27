# 🤔 About

Various utilities to manage photos.
Supported photo formats are PNG, JPG and HEIC.

___

## 🔹DuplicatePhotosFinder

Process photos from a given folder (and all its sub-folders), and generate a csv file reporting all duplicates.

```
DuplicatePhotosFinder [--directory <directory>] [--output <filename>.csv]
```

&nbsp;&nbsp;&nbsp;&nbsp;_directory_: The path of the directory to process. Current directory by default.

&nbsp;&nbsp;&nbsp;&nbsp;_output_: The path of the output csv file to generate. Default is `./duplicate.csv`.

___

## 🔹SortPhotos

Sort photos into folders named using date and location from photo Exif data.

```
SortPhotos [--directory <directory>] [--output <output>] [--location] [--undefined <name>] [--copy]
```

&nbsp;&nbsp;&nbsp;&nbsp;_directory_: The path of the directory to process. Current directory by default.

&nbsp;&nbsp;&nbsp;&nbsp;_output_: The path of the directory to copy or move sorted photo into. Default is `./SORTED`.

&nbsp;&nbsp;&nbsp;&nbsp;_location_: Use location in addition to date to name folders.

&nbsp;&nbsp;&nbsp;&nbsp;_undefined_: The folder name to use when Exif data cannot be retrieved. Modification date will be used in such case. Default is `UNDEFINED`.

&nbsp;&nbsp;&nbsp;&nbsp;_copy_: Copy the photos instead of moving them.

# 📝 Implementation

![LANGUAGE](https://img.shields.io/badge/python-royalblue?style=for-the-badge&logo=python&logoColor=white)
![EDITOR](https://img.shields.io/badge/vscode-coral?style=for-the-badge&logo=visual-studio-code&logoColor=white)
![OS](https://img.shields.io/badge/linux-yellowgreen?style=for-the-badge&logo=linux&logoColor=white)

## ⚙ Installation

Clone the repository and go to the project directory.

 ``` bash
 git clone https://github.com/ZooterOne/File-Utils
 cd File-Utils/PythonUtils
 ```

Setup the Python environment.

 ``` bash
 python -m venv .venv
 source .venv/bin/activate
 pip install -r requirements.txt 
 ```

 ## 🏃‍♂️ Run script
 
 ``` bash
 python <script>.py <parameters>
 ```

## 🧪 Run tests

 ``` bash
 python -m unittest -v
 ```

# 💡 Future developments

 - Use perceptual hash algorithm to detect duplicate photos.