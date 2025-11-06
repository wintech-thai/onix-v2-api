#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']

apiUrl = "api/Limit/org/#{orgId}/action/GetLimits"
param =  nil

result = make_request(:get, apiUrl, param)
puts(result)


apiUrl = "api/Limit/org/#{orgId}/action/GetLimitCount"
param =  nil

result = make_request(:get, apiUrl, param)
puts(result)
