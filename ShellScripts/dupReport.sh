#!/bin/bash

syntax()
{
  echo "Syntax: dupReport <files> [-o <filename>] [-h]"
  echo
  echo "Options:"
  echo "  h               Print this help."
  echo "  o <filename>    Output report file."
  echo
}

help()
{
  echo "Report duplicate files from previously generated directory cache."
  echo "  Cache files can be generated using dupGenerateCache."
  echo
  syntax
}

output=report.csv
inputs=""

while [ $OPTIND -le "$#" ]
do
  if getopts o::h option
    then
      case "${option}" in
        o) output=${OPTARG};;
        h) help
           exit;;
        \?) echo "[ERROR]: Invalid option."
            echo
            syntax
            exit;;
      esac
    else
      input="${!OPTIND}"
      if [ ! -e "${input}" ]; then
        echo "File ${input} does not exist."
        exit
      fi
      inputs="${inputs} ${input}"
      ((OPTIND++))
    fi
done

if [ -z "${inputs}" ]; then
  echo "No input cache file provided."
  echo
  syntax
  exit
fi

echo "Retrieving duplicates for$inputs."
echo "Saving report to $output."

cat $inputs | sort | uniq -w64 -D | awk '!($1 in a) {print ""; a[$1]}; {gsub($1, ""); print}' > "$output"
