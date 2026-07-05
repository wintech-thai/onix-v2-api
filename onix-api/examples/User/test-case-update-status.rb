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
status = "Open" # New, Open, InProgress, WaitingForCustomer, Resolved, Closed, Cancelled
apiUrl = "api/CaseManagement/org/#{orgId}/action/UpdateCaseStatus/#{caseId}/#{status}"

param = nil

result = make_request(:post, apiUrl, param)
puts(result.to_json)
