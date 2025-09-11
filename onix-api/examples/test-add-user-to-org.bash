#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X POST ${API_HTTP_ENDPOINT}/api/Organization/org/${API_ORG}/action/AddUserToOrganization \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @user-org.json
