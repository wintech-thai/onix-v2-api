#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X GET ${API_HTTP_ENDPOINT}/api/Job/org/${API_ORG}/action/GetJobTemplate/ScanItemGenerator \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
