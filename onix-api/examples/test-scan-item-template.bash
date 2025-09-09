#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X POST ${API_HTTP_ENDPOINT}/api/ScanItemTemplate/org/${API_ORG}/action/AddScanItemTemplate \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @scan-item-template.json
