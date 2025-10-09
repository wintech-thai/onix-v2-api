#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X GET ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/GetItemImageUploadPresignedUrl/99b5dbd5-7a79-4560-9a89-8c24b0393229 \
-H "Content-Type: application/json" \
-u "dummy:${API_KEY}" \

