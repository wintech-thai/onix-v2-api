#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/ScanItem/org/#{orgId}/action/GetScanItems"
param =  {
  FullTextSearch: ""
}

result = make_request(:post, apiUrl, param)
puts(result)


apiUrl = "api/ScanItem/org/#{orgId}/action/GetScanItemCount"
result = make_request(:post, apiUrl, param)
puts(result)
