#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
id = 'fe21126e-51d3-42f5-98d1-3152634c291b'

param =  nil

apiUrl = "api/ApiKey/org/#{orgId}/action/DisableApiKeyById/#{id}"
result = make_request(:post, apiUrl, param)

puts(result)
