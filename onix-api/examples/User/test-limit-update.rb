#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
statCode = "ScanItemBalanceCurrent"

apiUrl = "api/Limit/org/#{orgId}/action/UpdateLimit/#{statCode}/1500000"
param =  nil

result = make_request(:post, apiUrl, param)
puts(result)
