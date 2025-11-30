#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = "napbiotec"
scanItemId = "28414671-f33e-46b5-bc95-392a229dd77a"

### DeleteItemImagesByItemId
apiUrl = "api/ScanItem/org/#{orgId}/action/DetachScanItemFromProduct/#{scanItemId}"

result = make_request(:post, apiUrl, nil)
puts(result)

