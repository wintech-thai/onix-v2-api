#!/bin/bash

export $(grep -v '^#' .env | xargs)

#CreateJobScanItemGenerator

curl -s -X POST ${API_HTTP_ENDPOINT}/api/Job/org/${API_ORG}/action/CreateJobOtpEmailSend \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @job-email.json

#-d @job.json
