#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/PointRule/org/#{orgId}/action/GetPointRules"
param =  {
  FullTextSearch: "",
}

result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/PointRule/org/#{orgId}/action/GetPointRulesCount"
result = make_request(:post, apiUrl, param)
puts(result)
