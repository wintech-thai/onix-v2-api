#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/DocumentNumber/org/#{orgId}/action/GetDocumentNumberConfigs"
param = {
  FullTextSearch: "",
  Limit: 100,
  Offset: 0,
}

result = make_request(:post, apiUrl, param)
json = result.to_json
puts(json)

apiUrl = "api/DocumentNumber/org/#{orgId}/action/GetDocumentNumberConfigCount"
result = make_request(:post, apiUrl, param)
json = result.to_json
puts("Count: #{json}")
