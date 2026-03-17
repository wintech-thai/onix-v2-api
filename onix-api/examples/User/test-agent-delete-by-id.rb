#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'c7324536-67b6-420a-97bc-deea9178f1b3'

apiUrl = "api/Agent/org/#{orgId}/action/DeleteAgentById/#{id}"
param = nil

result = make_request(:delete, apiUrl, param)

json = result.to_json
puts(json)
