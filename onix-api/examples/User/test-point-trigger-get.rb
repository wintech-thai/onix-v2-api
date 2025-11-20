#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/PointTrigger/org/#{orgId}/action/GetPointTriggers"
param =  {
  FullTextSearch: "",
}

result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/PointTrigger/org/#{orgId}/action/GetPointTriggersCount"
result = make_request(:post, apiUrl, param)
puts(result)
