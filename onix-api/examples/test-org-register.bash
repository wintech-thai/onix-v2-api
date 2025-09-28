#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X POST ${API_HTTP_ENDPOINT}/api/OnlyAdmin/org/global/action/RegisterOrganization \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY_GLOBAL}" \
-d @org.json
