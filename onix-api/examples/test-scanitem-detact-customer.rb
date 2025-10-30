#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = "napbiotec"
scanItemId = "7bcd3f22-4d84-4ac1-b1d6-9c4754716544"

### DeleteItemImagesByItemId
apiUrl = "api/ScanItem/org/#{orgId}/action/DetachScanItemFromCustomer/#{scanItemId}"

result = make_request(:post, apiUrl, nil)
puts(result)

