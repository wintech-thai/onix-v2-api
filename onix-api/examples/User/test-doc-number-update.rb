#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("../.env")

orgId = ENV['API_ORG']
id = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx'  # replace with actual id

apiUrl = "api/DocumentNumber/org/#{orgId}/action/UpdateDocumentNumberConfigById/#{id}"
param = {
  DocumentFormat: "TMP-${yyyy}-${mm}-${seq}",
  SeqDigit: 5,
  ResetType: "Yearly",
  YearOffset: 543,
  Tags: "testing,updated",
}

result = make_request(:post, apiUrl, param)
json = result.to_json
puts(json)
