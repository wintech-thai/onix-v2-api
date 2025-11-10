#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/Stat/org/#{orgId}/action/GetStats"
param =  {
  StatCode: "BalanceCurrent",
}

result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/Stat/org/#{orgId}/action/GetStatCount"
result = make_request(:post, apiUrl, param)
puts(result)
