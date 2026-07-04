#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']

###
caseId = "your-case-uuid-here"
apiUrl = "api/CaseManagement/org/#{orgId}/action/GetCaseById/#{caseId}"

param = nil

result = make_request(:get, apiUrl, param)
puts(result.to_json)
