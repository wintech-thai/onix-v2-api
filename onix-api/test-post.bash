#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X POST ${API_HTTP_ENDPOINT}/api/Job/org/${API_ORG}/action/CreateJobScanItemGenerator \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @test-post.json
