#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

param =  {
  Roles: [ "VIEWER" ],
  KeyDescription: "Test API Key from Ruby script",
}

apiUrl = "api/ApiKey/org/#{orgId}/action/AddApiKey"
result = make_request(:post, apiUrl, param)

puts(result)
