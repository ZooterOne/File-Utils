#!/bin/bash

syntax()
{
  echo "Syntax: dupGenerateCache [-d <directory>] [-o <filename>] [-h]"
  echo
  echo "Options:"
  echo "  h               Print this help."
  echo "  d <directory>   Input directory."
  echo "  o <filename>    Output cache file."
  echo
}

help()
{
  echo "Generate the cache for a directory and its sub-directories."
  echo
  syntax
}

directory=./

while getopts d:o::h option
do
  case "${option}" in
    d) directory=${OPTARG};;
    o) output=${OPTARG};;
    h) help
       exit;;
    \?) echo "[ERROR]: Invalid option."
        syntax
        exit;;
  esac
done

if [ ! -d "$directory" ]; then
  echo "Directory $directory does not exist."
fi

if [ -z "$output" ]; then
  directoryName="$(basename $directory)"
  if [ "$directoryName" == "." ]; then
    cache="$(pwd).cache"
  elif [ "$directoryName" == ".." ]; then
    cache="directory.cache"
  else
    cache="$directoryName.cache"
  fi 
else
  cache="$output"
fi

echo "Generating cache for $directory."
echo "Saving cache to $cache."

rgx='.*\.\(png\|jpg\|jpeg\|heic\|mp4\|avi\)'

find $directory -type f -iregex "$rgx" -exec sha256sum {} \; | awk '{gsub("'$directory'", ""); print}' | sort > "$cache"
