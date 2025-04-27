# 🤔 About

Tools to find duplicate files within a directory and its sub-directories.

## Commands

```
duplicates [<folder>] [--cache <cache>]
```

Find all duplicate files within a directory and its sub-directories.

```
find <file> [<directory>] [--cache <cache>]
```

Find duplicates of a given file within a directory and its sub-directories.

```
build [<directory>] [<filename>]
```

Build the cache for a directory and its sub-directories.

```
report [--cache <files>] [--output <filename>] [--distinct]
```

Compare the cache for multiple directories and report the duplicates.

# 📝 Implementation

![LANGUAGE](https://img.shields.io/badge/dotnet-royalblue?style=for-the-badge&logo=dotnet&logoColor=white)
![EDITOR](https://img.shields.io/badge/rider-coral?style=for-the-badge&logo=rider&logoColor=white)
![OS](https://img.shields.io/badge/linux-yellowgreen?style=for-the-badge&logo=linux&logoColor=white)

Project uses [Spectre.Console](https://spectreconsole.net/) library to manage command line options and user experience.

## ⚙ Installation

Clone the repository and go to the project directory.

 ``` bash
 git clone https://github.com/ZooterOne/File-Utils
 cd File-Utils/dotNetTools
 ```

Build the project.

 ``` bash
 dotnet build 
 ```

 ## 🏃‍♂️ Run tool
 
 ``` bash
 dotnet run --project DuplicateFinder.CLI <command> <parameters>
 ```

## 🧪 Run tests

 ``` bash
 dotnet test
 ```
