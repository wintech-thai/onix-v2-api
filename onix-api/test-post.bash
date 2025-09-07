#!/bin/bash

curl -s -X POST http://localhost:5102/api/Job/org/default/action/CreateJobScanItemGenerator \
-H "Content-Type: application/json" \
-v \
-u "dummy:bdb10fe1-04bc-438e-8cb3-318e505f73f9" \
-d @test-post.json
