#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '6978a49b-66a0-4511-b4de-37059caf1c33'

param =  nil

apiUrl = "api/ApiKey/org/#{orgId}/action/DeleteApiKeyById/#{id}"
result = make_request(:delete, apiUrl, param)

puts(result)
