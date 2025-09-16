#!/bin/bash

export $(grep -v '^#' .env | xargs)

#curl -s -X GET ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/GetItemById/f7ae642d-884f-4075-a63a-4708cd3467a3 \
#-H "Content-Type: application/json" \
#-v \
#-u "dummy:${API_KEY}" \

curl -s -X POST ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/AddItemImage/99b5dbd5-7a79-4560-9a89-8c24b0393229 \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @item-image.json

curl -s -X GET ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/GetItemImagesByItemId/99b5dbd5-7a79-4560-9a89-8c24b0393229 \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
