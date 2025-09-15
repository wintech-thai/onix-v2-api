#!/bin/bash

export $(grep -v '^#' .env | xargs)

#curl -s -X GET ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/GetItemById/4510d3f7-5d01-4893-af33-cd45bd35fd59 \
#-H "Content-Type: application/json" \
#-v \
#-u "dummy:${API_KEY}" \


curl -s -X GET ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/GetItemImagesByItemId/4510d3f7-5d01-4893-af33-cd45bd35fd59 \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \


curl -s -X POST ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/AddItemImage/4510d3f7-5d01-4893-af33-cd45bd35fd59 \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \
-d @item-image.json
