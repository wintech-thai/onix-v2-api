#!/bin/bash

export $(grep -v '^#' .env | xargs)

JSON_FILE_SRC=scan-item-action.json
JSON_FILE_TMP=scan-item-action.json.tmp

cp ${JSON_FILE_SRC} ${JSON_FILE_TMP}

sed -i "s#<ENCRYPTION_KEY>#${ENCRYPTION_KEY}#g" ${JSON_FILE_TMP}
sed -i "s#<ENCRYPTION_IV>#${ENCRYPTION_IV}#g" ${JSON_FILE_TMP}

curl -s -X POST ${API_HTTP_ENDPOINT}/api/ScanItemAction/org/${API_ORG}/action/AddScanItemAction \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @${JSON_FILE_TMP}

rm ${JSON_FILE_TMP}
