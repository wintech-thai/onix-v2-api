#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/ScanItemAction/org/#{orgId}/action/GetScanItemActions"
param = {
  FullTextSerch: ""
}
result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/ScanItemAction/org/#{orgId}/action/GetScanItemActionCount"
result = make_request(:post, apiUrl, param)
puts(result)