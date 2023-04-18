#!/bin/bash
CMDNAME=`basename $0`
if [ $# -ne 2 ]; then
  echo "Usage: $CMDNAME <Unity Project Path> <API Key>"
  exit 1
fi

filepath="$1/ProjectSettings/ARCoreExtensionsProjectSettings.json"

keys=(
    "AndroidCloudServicesApiKey"
    "IOSCloudServicesApiKey"
)

for key in "${keys[@]}" ; do
    sed -i -e "s/\"${key}\"[ ]*:[ ]*\"[^\"]*\"/\"${key}\":\"$2\"/" ${filepath}
done