#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']

apiUrl = "api/Point/org/#{orgId}/action/GetWallets"
param =  {
  FullTextSearch: "xxx",
}

result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/Point/org/#{orgId}/action/GetWalletsCount"
result = make_request(:post, apiUrl, param)
puts(result)
