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
productId = "99b5dbd5-7a79-4560-9a89-8c24b0393229"

### DeleteItemImagesByItemId
apiUrl = "api/ScanItem/org/#{orgId}/action/AttachScanItemToProduct/#{scanItemId}/#{productId}"

result = make_request(:post, apiUrl, nil)
puts(result)
