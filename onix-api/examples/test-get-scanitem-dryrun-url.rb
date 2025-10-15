#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = "napbiotec"
scanItemId = "e2eb2b18-6435-4c36-8c0a-04861b35fac9"

### GetScanItemUrlDryRunById
apiGetScanItemUrlDryRun = "api/ScanItem/org/#{orgId}/action/GetScanItemUrlDryRunById/#{scanItemId}"

result = make_request(:get, apiGetScanItemUrlDryRun, nil)
puts(result)
