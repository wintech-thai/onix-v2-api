#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X POST ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/AddItem \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @item.json
