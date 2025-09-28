#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X GET ${API_HTTP_ENDPOINT}/api/OnlyAdmin/org/global/action/SendOrgRegisterOtpEmail/pjame16.fb@gmail.com \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY_GLOBAL}" \
