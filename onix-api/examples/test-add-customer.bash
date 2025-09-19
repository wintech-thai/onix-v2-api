#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X POST ${API_HTTP_ENDPOINT}/api/Customer/org/${API_ORG}/action/AddCustomer \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @customer.json
