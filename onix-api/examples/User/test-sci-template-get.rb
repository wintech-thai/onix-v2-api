#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/ScanItemTemplate/org/#{orgId}/action/GetScanItemTemplates"
param = {
  FullTextSerch: ""
}
result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/ScanItemTemplate/org/#{orgId}/action/GetScanItemTemplateCount"
result = make_request(:post, apiUrl, param)
puts(result)