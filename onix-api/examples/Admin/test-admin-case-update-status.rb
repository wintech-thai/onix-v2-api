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
caseId = "your-case-uuid-here"
status = "Open" # New, Open, InProgress, WaitingForCustomer, Resolved, Closed, Cancelled
apiUrl = "admin-api/AdminCaseManagement/org/global/action/UpdateCaseStatus/#{caseId}/#{status}"

param = nil

token = File.read(keyFile)

ENV['API_KEY'] = nil
ENV['ACCESS_TOKEN'] = token

result = make_request(:post, apiUrl, param)
puts(result.to_json)
