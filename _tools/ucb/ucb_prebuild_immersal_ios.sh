#!/bin/bash

cd $(dirname $0)
project_path="../../unity/SpirareBrowser-ARFoundation-Immersal"
../setup_immersal_import.sh ${project_path} ${IMMERSAL_SDK_DOWNLOAD_URL}