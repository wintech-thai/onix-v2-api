#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'  # replace with actual id

apiUrl = "api/DocumentNumber/org/#{orgId}/action/GetDocumentNumberConfigById/#{id}"

result = make_request(:get, apiUrl, nil)
json = result.to_json
puts(json)
