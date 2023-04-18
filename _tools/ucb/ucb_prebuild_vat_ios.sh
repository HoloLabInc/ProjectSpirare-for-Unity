#!/bin/bash

cd $(dirname $0)
project_path="../../unity/SpirareBrowser-ARFoundation-VAT"
../setup_vuforia_engine_import.sh ${project_path} ${VUFORIA_ENGINE_PACKAGE_NAME} ${VUFORIA_ENGINE_DOWNLOAD_URL}
../setup_vuforia_engine_license.sh ${project_path} ${VUFORIA_ENGINE_LICENSE_KEY}