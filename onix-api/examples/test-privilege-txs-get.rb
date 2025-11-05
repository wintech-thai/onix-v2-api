#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
itemId = "41316c06-9ab7-4ae3-a9a1-4810c3b1e79c"

apiUrl = "api/Privilege/org/#{orgId}/action/GetPrivilegeTxsById/#{itemId}"
param =  {}

result = make_request(:post, apiUrl, param)
puts(result)

apiUrl = "api/Privilege/org/#{orgId}/action/GetPrivilegeTxsCountById/#{itemId}"
result = make_request(:post, apiUrl, param)
puts(result)
