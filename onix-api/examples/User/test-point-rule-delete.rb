#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'c1f098a3-b74d-4b01-9c0a-09a1276c6c73'

apiUrl = "api/PointRule/org/#{orgId}/action/DeletePointRuleById/#{id}"
param = nil

result = make_request(:delete, apiUrl, param)
puts(result)
