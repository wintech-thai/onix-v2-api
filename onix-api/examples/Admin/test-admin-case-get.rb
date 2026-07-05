#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

keyFile = ".token"

###
apiUrl = "admin-api/AdminCaseManagement/org/global/action/GetCases"
param = {
  FullTextSearch: "",
  Status: "",
  Priority: "",
  OrgIdFilter: "",
  PageNo: 1,
  PageSize: 20,
}

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:post, apiUrl, param)
puts(result.to_json)
