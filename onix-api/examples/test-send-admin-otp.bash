#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X GET ${API_HTTP_ENDPOINT}/api/Admin/org/global/action/SendOrgRegisterOtpEmail/pjame.fb@gmail.comxxx \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY_GLOBAL}" \
