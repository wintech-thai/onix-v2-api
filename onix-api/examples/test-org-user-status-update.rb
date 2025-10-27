#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env(".env")

orgId = ENV['API_ORG']
id = '6ff52772-3389-466f-b6e8-cbce250f4df7'

param = nil

### UpdateUserById
apiUrl = "api/OrganizationUser/org/#{orgId}/action/EnableUserById/#{id}"
result = make_request(:post, apiUrl, param)

puts(result)
