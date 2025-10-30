#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = "napbiotec"
scanItemId = "1f28e524-07b3-4781-bfff-b9a6c4cb2d1c"
customerId = "13f5b799-71b6-4c14-bbf8-2ba59d5ce477"

### DeleteItemImagesByItemId
apiUrl = "api/ScanItem/org/#{orgId}/action/AttachScanItemToCustomer/#{scanItemId}/#{customerId}"

result = make_request(:post, apiUrl, nil)
puts(result)
