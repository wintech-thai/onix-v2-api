#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X GET ${API_HTTP_ENDPOINT}/org/${API_ORG}/GetProduct/${SERIAL}/${PIN} \
-H "Content-Type: application/json" \
-v \
#-u "dummy:${API_KEY}" \
