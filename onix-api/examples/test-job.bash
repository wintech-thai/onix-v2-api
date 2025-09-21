#!/bin/bash

export $(grep -v '^#' .env | xargs)

#CreateJobScanItemGenerator
#CreateJobOtpEmailSend

curl -s -X POST ${API_HTTP_ENDPOINT}/api/Job/org/${API_ORG}/action/CreateJobCacheLoaderTrigger \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @job-cache-load.json

#-d @job-email.json
#-d @job.json
