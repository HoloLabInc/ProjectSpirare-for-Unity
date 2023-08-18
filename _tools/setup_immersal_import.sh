#!/bin/bash
CMDNAME=`basename $0`
if [ $# -ne 2 ]; then
  echo "Usage: $CMDNAME <Unity Project Path> <Download URL>"
  exit 1
fi

url="$2"
destination_directory="$1/Assets"

filename="ImmersalSDK.zip"

curl -L -o "$filename" "$url" ${CURL_OPTIONS}
unzip -o "$filename" -d "$destination_directory"
