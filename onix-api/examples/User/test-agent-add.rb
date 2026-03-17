#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/Agent/org/#{orgId}/action/AddAgent"
param = {
  Code: "SENSOR-003",
  Description: "Test sensor #1",
  Tags: "testing",
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
