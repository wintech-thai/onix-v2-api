#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
keyFile = ".token"

### Get Merchant Dashboard
apiUrl = "api/Report/org/#{orgId}/action/GetMerchantDashboard"
param =  {
  Limit: 100,
  Offset: 0
}

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

puts("===== Get Merchant Dashboard =====")
result = make_request(:post, apiUrl, param)
puts(result)

### Get Merchant Analytics
apiUrl = "api/Report/org/#{orgId}/action/GetMerchantAnalytics"

puts("\n===== Get Merchant Analytics =====")
result = make_request(:post, apiUrl, param)
puts(result)
