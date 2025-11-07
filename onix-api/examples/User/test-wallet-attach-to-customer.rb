#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
walletId = "4d9fdd49-307b-428c-8c2e-927d27ef258d"
customerId = "60688506-ef7d-4f0b-9da0-1a182bcf6e9c"

### 
apiUrl = "api/Point/org/#{orgId}/action/AttachCustomerToWalletById/#{walletId}/#{customerId}"

result = make_request(:post, apiUrl, nil)
puts(result)

