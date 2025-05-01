# 🤔 About

Collection of utilities to manage files, mainly regarding detecting duplicate files.

# 💡 Details

The process to find duplicate files is simple:

 1. Get the files to compare.
 2. For each file, generate the SHA256 hash code from its content.
 3. Compare the generated codes and report the files with the same hash code.

# 📝 Implementation

The aim of the project is to learn different ways of handling files, using different languages.

The project is composed of the following implementations:

[![PYTHON](https://img.shields.io/badge/python-cornflowerblue?style=for-the-badge&logo=python&logoColor=white)](PythonUtils/README.md)
[![DOTNET](https://img.shields.io/badge/dotnet-royalblue?style=for-the-badge&logo=dotnet&logoColor=white)](dotNetTools/README.md)
[![BASH](https://img.shields.io/badge/shell-steelblue?style=for-the-badge&logo=linux&logoColor=white)](ShellScripts/README.md)

# 🧪 Results

Results using a laptop with an i7-7700HQ CPU, 8GB RAM, under Fedora 42.
Dataset contains 882 images. All tools returning the same number of duplicate files (450).

| __Implementation__ | __Total time__ |
| --- | --- |
| ![PYTHON](https://skillicons.dev/icons?i=py) | 15 seconds |
| ![DOTNET](https://skillicons.dev/icons?i=cs,dotnet) | 18 seconds |
| ![SHELL](https://skillicons.dev/icons?i=bash) | 34 seconds |
