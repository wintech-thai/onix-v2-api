#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = '339ffabf-3e9a-44c1-9009-a0fc5d99e4e5'

apiUrl = "api/CustomRole/org/#{orgId}/action/DeleteCustomRoleById/#{id}"
param = nil

result = make_request(:delete, apiUrl, param)

json = result.to_json
puts(json)
