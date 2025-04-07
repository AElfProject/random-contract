#!/bin/bash

set -e

ContractsName=$1
VERSION=$2

[ -z ${VERSION} ] && { echo "Usage: $0 <version>"; exit 1; }

[ -d "/opt/build/${ContractsName}" ] && rm -rf /opt/build/${ContractsName}/*

cd /opt/contracts

dotnet build \
  src/${ContractsName}.csproj \
  /p:NoBuild=false \
  /p:Version=${VERSION} \
  -c Release \
  -o /opt/build/${ContractsName}-${VERSION}

