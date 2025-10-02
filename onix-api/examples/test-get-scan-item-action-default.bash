#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X GET ${API_HTTP_ENDPOINT}/api/ScanItemAction/org/${API_ORG}/action/GetScanItemActionDefault \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
