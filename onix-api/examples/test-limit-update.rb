#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
statCode = "CustomerBalanceDaily"

apiUrl = "api/Limit/org/#{orgId}/action/UpdateLimit/#{statCode}/1500"
param =  nil

result = make_request(:post, apiUrl, param)
puts(result)
