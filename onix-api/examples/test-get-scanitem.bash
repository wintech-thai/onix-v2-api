#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X POST ${API_HTTP_ENDPOINT}/api/ScanItem/org/${API_ORG}/action/GetScanItems \
-H "Content-Type: application/json" \
-u "dummy:${API_KEY}" \
-d @scanitem.json
