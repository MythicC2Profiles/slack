#!/bin/bash

cd /Mythic/c2_code/src/slack-server
dotnet restore
dotnet publish -c Release -o /Mythic/c2_code
mv /Mythic/c2_code/slack-server /Mythic/c2_code/server

cd /Mythic/mythic

export PYTHONPATH=/Mythic:/Mythic/mythic

python3.8 mythic_service.py