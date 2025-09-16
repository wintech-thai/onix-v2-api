#!/bin/bash

export $(grep -v '^#' .env | xargs)

#curl -s -X POST ${API_HTTP_ENDPOINT}/api/ScanItem/org/${API_ORG}/action/AttachScanItemToProduct/54a011ab-c6b3-4022-b940-069db23cdcd2/be90fb05-2608-4094-a464-6e23825231e4 \
#-H "Content-Type: application/json" \
#-v \
#-u "dummy:${API_KEY}" \

