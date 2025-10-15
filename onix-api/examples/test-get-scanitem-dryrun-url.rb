#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = "napbiotec"
scanItemId = "0ff730a0-f2a8-47a8-b9fc-b45efdcc1254"

### GetScanItemUrlDryRunById
apiGetScanItemUrlDryRun = "api/Item/org/#{orgId}/action/GetScanItemUrlDryRunById/#{scanItemId}"

result = make_request(:get, apiGetScanItemUrlDryRun, nil)
puts(result)
