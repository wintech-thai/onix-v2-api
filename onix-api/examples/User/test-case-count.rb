#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/CaseManagement/org/#{orgId}/action/GetCaseCount"
param = {
  FullTextSearch: "",
  Status: "",
  Priority: "",
}

result = make_request(:post, apiUrl, param)
puts(result.to_json)
