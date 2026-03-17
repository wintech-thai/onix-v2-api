#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/Agent/org/#{orgId}/action/GetAgents"
param = {
  FullTextSearch: "",
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)

apiUrl = "api/Agent/org/#{orgId}/action/GetAgentCount"
result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
