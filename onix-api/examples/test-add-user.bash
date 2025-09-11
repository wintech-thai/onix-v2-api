#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X POST ${API_HTTP_ENDPOINT}/api/User/org/${API_ORG}/action/AddUser \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @user.json
