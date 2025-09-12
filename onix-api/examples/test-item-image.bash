#!/bin/bash

export $(grep -v '^#' .env | xargs)

curl -s -X GET ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/GetItemById/be90fb05-2608-4094-a464-6e23825231e4 \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \


curl -s -X GET ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/GetItemImagesByItemId/be90fb05-2608-4094-a464-6e23825231e4 \
-H "Content-Type: application/json" \
-v \
-u "dummy:${API_KEY}" \


#curl -s -X POST ${API_HTTP_ENDPOINT}/api/Item/org/${API_ORG}/action/AddItemImage/be90fb05-2608-4094-a464-6e23825231e4 \
#-H "Content-Type: application/json" \
#-v \
#-u "dummy:${API_KEY}" \
#-d @item-image.json
