#!/bin/bash

export $(grep -v '^#' .env | xargs)

ACCESS_TOKEN="eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJHOTV2ajMtZDlJQ0otNEJuZ09iUkY2MVlpNzBMR01jQkpuZklISmFoRkFJIn0.eyJleHAiOjE3NTg1OTY4OTcsImlhdCI6MTc1ODU5NjU5NywianRpIjoiY2YzODAwMmItNWNlMy00OTQ3LWIyMWUtNmQ0ZjY3NWQ4ZjNiIiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5kZXZvcHMubmFwYmlvdGVjLmlvL2F1dGgvcmVhbG1zL29uaXgtdjItZGV2IiwiYXVkIjoiYWNjb3VudCIsInN1YiI6IjVjMTVjMjJiLWQwMTctNDhmYy1hYmE1LWQwYzNlZDNlNGI4OCIsInR5cCI6IkJlYXJlciIsImF6cCI6InBsZWFzZS1zY2FuIiwic2Vzc2lvbl9zdGF0ZSI6ImYwODM2MGYzLWZlMjctNGRlZi05N2JiLTRlYjVkNGQyM2ZhNyIsImFjciI6IjEiLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsib2ZmbGluZV9hY2Nlc3MiLCJkZWZhdWx0LXJvbGVzLW9uaXgtdjItZGV2IiwidW1hX2F1dGhvcml6YXRpb24iXX0sInJlc291cmNlX2FjY2VzcyI6eyJhY2NvdW50Ijp7InJvbGVzIjpbIm1hbmFnZS1hY2NvdW50IiwibWFuYWdlLWFjY291bnQtbGlua3MiLCJ2aWV3LXByb2ZpbGUiXX19LCJzY29wZSI6Im9wZW5pZCBwcm9maWxlIGVtYWlsIG9mZmxpbmVfYWNjZXNzIiwic2lkIjoiZjA4MzYwZjMtZmUyNy00ZGVmLTk3YmItNGViNWQ0ZDIzZmE3IiwiZW1haWxfdmVyaWZpZWQiOmZhbHNlLCJuYW1lIjoiUGphbWUgUGxlYXNlLVNjYW4iLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJwamFtZW5hamExNCIsImdpdmVuX25hbWUiOiJQamFtZSIsImZhbWlseV9uYW1lIjoiUGxlYXNlLVNjYW4iLCJlbWFpbCI6InBqYW1lMTQuZmJAZ21haWwuY29tIn0.l0CaXI_JdRB4hBOiJ9PehnKGIvbISW7v0l2WB42eyATHrrdxGGN3FfH9JcPAz68Wyim4Hjo1C5fM-3vwWZQ6NvfCAu7-f6I-D3p4-R9PwjMekvwF4m-aC7IB4pXJF9roxwNyZYSi2uTMZLywOZO2T62SMuMmFRHA-XAjrMEG3tablo5mh2nMc2KB4OUIHv-7vu6-lf9Ze6kv_SO4356sTpaocXBEYKRXqWntK8K_-UEu4OA8jhZgG6W1PSNtCak8qNQ3KnxwtmDBtu-Td822J4b0ytqCPcIKuKVZk2P0SL486R-NsggKGoOEfcN4p19y8Zzma8GwEUpswi21OFHxFw"
ACCESS_TOKEN_B64=$(echo -n "${ACCESS_TOKEN}" | base64 -w0)

#curl -s -X GET ${API_HTTP_ENDPOINT}/api/Organization/org/${API_ORG}/action/GetUserAllowedOrg \
#-H "Content-Type: application/json" -v -u "dummy:${API_KEY}"

API_ENDPOINT='/api/User/org/temp/action/UpdatePassword'

curl -s -X POST "${API_HTTP_ENDPOINT}${API_ENDPOINT}" \
-H "Content-Type: application/json" -v \
-H "Authorization: Bearer ${ACCESS_TOKEN_B64}" \
-d @update-password.json \

#curl -s -X POST https://keycloak.devops.napbiotec.io/auth/realms/onix-v2-dev/account/credentials/password \
#-H "Content-Type: application/json" -v \
#-H "Authorization: Bearer ${ACCESS_TOKEN}" \
#-d @update-password.json \
