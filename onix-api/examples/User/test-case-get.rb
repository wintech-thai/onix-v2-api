#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

apiUrl = "api/CaseManagement/org/#{orgId}/action/GetCases"
param = {
  FullTextSearch: "",
  Status: "",
  Priority: "",
  PageNo: 1,
  PageSize: 20,
}

result = make_request(:post, apiUrl, param)
puts(result.to_json)
