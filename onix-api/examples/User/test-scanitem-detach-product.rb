#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = "napbiotec"
scanItemId = "a1ca3fb0-700b-4558-a79b-446763a2756c"

### DeleteItemImagesByItemId
apiUrl = "api/ScanItem/org/#{orgId}/action/DetachScanItemFromProduct/#{scanItemId}"

result = make_request(:post, apiUrl, nil)
puts(result)

