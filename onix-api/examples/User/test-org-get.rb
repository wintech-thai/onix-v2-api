#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

param = nil

apiUrl = "api/Organization/org/#{orgId}/action/GetOrganization"
result = make_request(:get, apiUrl, param)
puts(result)
