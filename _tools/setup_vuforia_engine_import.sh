#!/bin/bash
CMDNAME=`basename $0`
if [ $# -ne 3 ]; then
  echo "Usage: $CMDNAME <Unity Project Path> <Vuforia Engine Package Name> <Download URL>"
  exit 1
fi

filepath="$1/Packages/$2"
download_url="$3"

curl -o ${filepath} -L ${download_url}

# Update manifest.json
manifest_file="$1/Packages/manifest.json"
new_dependency="\"com.ptc.vuforia.engine\": \"file:$2\","
temp_file=$(mktemp)

if [ ! -f "${manifest_file}" ]; then
  echo "Error: Input file does not exist."
  exit 1
fi

awk -v new_dependency="${new_dependency}" '{
  print
  if ($0 ~ /[[:space:]]*"dependencies"[[:space:]]*:[[:space:]]*{/) {
    print "    " new_dependency
  }
}' "${manifest_file}" > "${temp_file}"

mv "${temp_file}" "${manifest_file}"

