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

### Get Merchant Summary
apiUrl = "api/Summary/org/#{orgId}/action/GetMerchantSummary"
param =  {
  Limit: 100,
  Offset: 0
}

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

puts("===== Get Merchant Summary =====")
result = make_request(:post, apiUrl, param)
puts(result)

### Get Revenue Summary
apiUrl = "api/Summary/org/#{orgId}/action/GetRevenueSummary"

puts("\n===== Get Revenue Summary =====")
result = make_request(:post, apiUrl, param)
puts(result)
