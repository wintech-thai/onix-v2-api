#!/bin/bash

export $(grep -v '^#' .env | xargs)

OTP=916594

curl -s -X POST ${API_HTTP_ENDPOINT}/org/${API_ORG}/RegisterCustomer/${SERIAL}/${PIN}/${OTP} \
-H "Content-Type: application/json" \
-v \
-d @customer.json
