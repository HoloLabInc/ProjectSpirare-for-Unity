#!/bin/bash
CMDNAME=`basename $0`
if [ $# -ne 2 ]; then
  echo "Usage: $CMDNAME <Unity Project Path> <Vuforia Engine License Key>"
  exit 1
fi

filepath="$1/Assets/Resources/VuforiaLicenseKeySettings.asset"
license_key=$2

cat << EOF > ${filepath}
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: a14db76b4698f1b498dfb0a2ca909aac, type: 3}
  m_Name: VuforiaLicenseKeySettings
  m_EditorClassIdentifier: 
  LicenseKey: ${license_key}
EOF
