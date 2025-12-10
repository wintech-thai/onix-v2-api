#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/ScanItemFolder/org/#{orgId}/action/GetScanItemFolders"
param =  {
  FullTextSearch: ""
}
result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/ScanItemFolder/org/#{orgId}/action/GetScanItemFolderCount"
result = make_request(:post, apiUrl, param)
puts(result)
