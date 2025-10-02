#!/bin/bash

export $(grep -v '^#' .env | xargs)

JSON_FILE_SRC=login.json
JSON_FILE_TMP=login.json.tmp

cp ${JSON_FILE_SRC} ${JSON_FILE_TMP}

sed -i "s#<AUTH_PASSWORD>#${AUTH_PASSWORD}#g" ${JSON_FILE_TMP}

curl -s -X POST ${API_HTTP_ENDPOINT}/api/Auth/org/temp/action/Login \
-H "Content-Type: application/json" \
-d @${JSON_FILE_TMP}

#curl -s -X POST ${API_HTTP_ENDPOINT}/api/Auth/org/temp/action/Refresh \
#-H "Content-Type: application/json" \
#-v \
#-d @${JSON_FILE_TMP}
